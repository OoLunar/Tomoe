using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Part of me honestly hopes that nobody hosts Tomoe except for me (Lunar). Please just make PRs to the repository instead 😭
    /// </summary>
    public static class SourceCodeCommand
    {
        private record PartialCommand
        {
            public required string ClassName { get; init; }
            public AttributeSyntax? CommandAttribute { get; set; }

            // We use a list of class declarations because a single file can have multiple group commands defined in it.
            public Dictionary<string, List<ClassDeclarationSyntax>> Members { get; init; } = [];
        }

        private static readonly FrozenDictionary<string, string> _commandLinks;

        static SourceCodeCommand()
        {
            Dictionary<string, string> commandLinks = [];
            Dictionary<string, PartialCommand> partialClasses = [];

            // Setup the assembly logic for reuse
            Assembly assembly = typeof(SourceCodeCommand).Assembly;

            // Get the base commit URL
            string baseUrl = $"{ThisAssembly.Git.Url}/blob/{ThisAssembly.Git.Commit}/src/";

            // foreach *.cs file found within the embedded sources
            // This only works because of the following csproj property:
            // <EmbeddedResource Include="$(ProjectRoot)/src/**/*.cs" LogicalName="%(EmbeddedResource.RecursiveDir)%(EmbeddedResource.Filename).cs" FileExtension=".cs" Condition="$(Configuration) == 'Release'" />
            // Include is the wildcard path to all the C# files in the src directory
            // LogicalName is the name of the file, but with the directory structure. The item metadata can be found here: https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-well-known-item-metadata?view=vs-2022
            // FileExtension is the file extension, which is always .cs
            // Condition is the configuration, which we only want to include Release
            // The Condition is important because when I'm developing, including the resources in Debug mode will break my intellisense.
            // Like it literally just won't load and even debugging breaks due to an OOM exception.
            // ALSO it wouldn't work in concept because the links would point to a commit that doesn't exist.
            foreach (string resourceFile in assembly.GetManifestResourceNames())
            {
                if (!resourceFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                using Stream manifestStream = assembly.GetManifestResourceStream(resourceFile) ?? throw new InvalidOperationException($"Failed to get the embedded resource {resourceFile}.");
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(manifestStream), CSharpParseOptions.Default);

                // Get all the classes in the file
                foreach (ClassDeclarationSyntax classDeclaration in GetClasses(syntaxTree.GetRoot().DescendantNodes()).Distinct())
                {
                    // Get the FQN of the class
                    string className = $"{GetNamespace(classDeclaration)}.{classDeclaration.Identifier}";

                    // Test to see if it has the Command attribute
                    AttributeSyntax? commandAttribute = null;
                    foreach (AttributeListSyntax attributeList in classDeclaration.AttributeLists)
                    {
                        foreach (AttributeSyntax attribute in attributeList.Attributes)
                        {
                            if (attribute.Name.ToString() == "Command")
                            {
                                commandAttribute = attribute;
                                break;
                            }
                        }
                    }

                    // If the class is a partial class, store it for later
                    if (!partialClasses.TryGetValue(className, out PartialCommand? partialCommand))
                    {
                        partialClasses[className] = partialCommand = new PartialCommand()
                        {
                            ClassName = className
                        };
                    }

                    // We can load other partial files that don't have the Command attribute,
                    // so when we do find the command attribute, we'll store it.
                    partialCommand.CommandAttribute ??= commandAttribute;
                    if (!partialCommand.Members.TryGetValue(resourceFile, out List<ClassDeclarationSyntax>? classDeclarations))
                    {
                        partialCommand.Members[resourceFile] = classDeclarations = [];
                    }

                    classDeclarations.Add(classDeclaration);
                }
            }

            // Iterate through all methods in the class
            foreach (PartialCommand partialCommand in partialClasses.Values)
            {
                foreach ((string fileName, List<ClassDeclarationSyntax> classDeclarations) in partialCommand.Members)
                {
                    foreach (ClassDeclarationSyntax classDeclaration in classDeclarations)
                    {
                        foreach (MemberDeclarationSyntax member in classDeclaration.Members)
                        {
                            if (member is not MethodDeclarationSyntax methodDeclaration)
                            {
                                continue;
                            }

                            // Get the method name
                            string methodName = methodDeclaration.Identifier.Text;

                            // Test to see if it has the Command attribute
                            AttributeSyntax? subCommandAttribute = null;
                            AttributeSyntax? groupCommandAttribute = null;
                            foreach (AttributeListSyntax attributeList in methodDeclaration.AttributeLists)
                            {
                                foreach (AttributeSyntax attribute in attributeList.Attributes)
                                {
                                    // Thankfully we won't need to worry about aliases since the text command processor's TryGetCommand will handle those for us.
                                    if (attribute.Name.ToString() == "Command")
                                    {
                                        subCommandAttribute = attribute;
                                        break;
                                    }
                                    else if (attribute.Name.ToString() == "DefaultGroupCommand")
                                    {
                                        groupCommandAttribute = attribute;
                                        break;
                                    }
                                }
                            }

                            // Find the beginning and ending lines of the method, starting as early as the XML docs and ending at the closing brace
                            int start = methodDeclaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                            int end = methodDeclaration.GetLocation().GetLineSpan().EndLinePosition.Line + 1;

                            StringBuilder commandNameBuilder = new();
                            if (partialCommand.CommandAttribute is not null
                                && partialCommand.CommandAttribute.ArgumentList is not null
                                && partialCommand.CommandAttribute.ArgumentList.Arguments.Count > 0)
                            {
                                commandNameBuilder.Append(partialCommand.CommandAttribute.ArgumentList.Arguments[0].ToString().Trim('"'));

                                // If the method is a group command, simply add it by the command name
                                if (groupCommandAttribute is not null)
                                {
                                    commandLinks[commandNameBuilder.ToString()] = $"{baseUrl}{fileName}#L{start}-L{end}";
                                }

                                // Append a space for the subcommands
                                commandNameBuilder.Append(' ');
                            }

                            if (subCommandAttribute is not null && subCommandAttribute.ArgumentList is not null && subCommandAttribute.ArgumentList.Arguments.Count > 0)
                            {
                                // Append the command name, which may or may not have a group command prepended already.
                                commandNameBuilder.Append(subCommandAttribute.ArgumentList.Arguments[0].ToString().Trim('"'));
                                commandLinks[commandNameBuilder.ToString()] = $"{baseUrl}{fileName}#L{start}-L{end}";
                            }
                        }
                    }
                }
            }

            _commandLinks = commandLinks.ToFrozenDictionary();
        }

        private static IEnumerable<ClassDeclarationSyntax> GetClasses(IEnumerable<SyntaxNode> nodes)
        {
            foreach (SyntaxNode node in nodes)
            {
                if (node is ClassDeclarationSyntax classDeclaration)
                {
                    yield return classDeclaration;
                }

                foreach (ClassDeclarationSyntax classDeclarationSyntax in GetClasses(node.ChildNodes()))
                {
                    yield return classDeclarationSyntax;
                }
            }
        }

        private static string GetNamespace(SyntaxNode syntaxNode) => syntaxNode.Parent switch
        {
            NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
            null => string.Empty,
            _ => GetNamespace(syntaxNode.Parent)
        };

        /// <summary>
        /// Sends a link to the repository which contains the code for the bot.
        /// </summary>
        [Command("source_code"), TextAlias("repository", "source", "code", "repo")]
        public static async ValueTask ExecuteAsync(CommandContext context, [RemainingText] string? commandName = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                await context.RespondAsync($"You can find my source code here: <{ThisAssembly.Project.RepositoryUrl}>");
                return;
            }
            else if (!context.Extension.GetProcessor<TextCommandProcessor>().TryGetCommand(commandName, context.Guild?.Id ?? 0, out _, out Command? command))
            {
                await context.RespondAsync($"I couldn't find a command named `{commandName}`.");
                return;
            }
            else if (!_commandLinks.TryGetValue(command.FullName, out string? link))
            {
                await context.RespondAsync($"I couldn't find the source code for the command `{command.FullName}`/`{commandName}`.");
                return;
            }
            else
            {
                await context.RespondAsync($"You can find the source code for the command `{command.FullName}` here: <{link}>");
            }
        }
    }
}
