using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
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
        private static readonly FrozenDictionary<string, string> _commandLinks;

        static SourceCodeCommand()
        {
            Dictionary<string, string> commandLinks = [];

            // Setup the assembly logic for reuse
            Assembly assembly = typeof(SourceCodeCommand).Assembly;

            // Get the base commit URL
            string baseUrl = $"{ThisAssembly.Git.Url}/blob/{ThisAssembly.Git.Commit}/src/";

            // foreach *.cs file found within the embedded sources
            foreach (string resourceFile in assembly.GetManifestResourceNames())
            {
                // if the file is a C# file
                if (!resourceFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // grab the file's contents
                string fileName = resourceFile.Remove(0, $"{assembly.GetName().Name}.".Length).Replace('.', '/').Replace("/cs", ".cs");
                using Stream manifestStream = assembly.GetManifestResourceStream(resourceFile) ?? throw new InvalidOperationException($"Failed to get the embedded resource {resourceFile}.");
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(manifestStream), CSharpParseOptions.Default);

                // Get all the classes in the file
                foreach (ClassDeclarationSyntax classDeclaration in GetClasses(syntaxTree.GetRoot().DescendantNodes()))
                {
                    // Get the class name
                    string className = classDeclaration.Identifier.Text;

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

                    // Iterate through all methods in the class
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
                        if (commandAttribute is not null && commandAttribute.ArgumentList is not null && commandAttribute.ArgumentList.Arguments.Count > 0)
                        {
                            commandNameBuilder.Append(commandAttribute.ArgumentList.Arguments[0].ToString().Trim('"'));

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

        /// <summary>
        /// Sends a link to the repository which contains the code for the bot.
        /// </summary>
        [Command("source_code"), TextAlias("repository", "source", "code", "repo")]
        public static async ValueTask ExecuteAsync(CommandContext context, string? commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                await context.RespondAsync($"You can find my source code here: <{ThisAssembly.Project.RepositoryUrl}>");
                return;
            }
            else if (!context.Extension.TryGetProcessor(out TextCommandProcessor? processor))
            {
                throw new UnreachableException("The text command processor was not found.");
            }
            else if (!processor.TryGetCommand(commandName, context.Guild?.Id ?? 0, out _, out Command? command))
            {
                await context.RespondAsync($"I couldn't find a command named `{commandName}`.");
                return;
            }
            else if (!_commandLinks.TryGetValue(command.FullName, out string? link))
            {
                await context.RespondAsync($"I couldn't find the source code for the command `{commandName}`.");
                return;
            }
            else
            {
                await context.RespondAsync($"You can find the source code for the command `{command.FullName}` here: <{link}>");
            }
        }
    }
}
