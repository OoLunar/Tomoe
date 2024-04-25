using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.EventArgs;
using OoLunar.XmlDocsNET;
using OoLunar.XmlDocsNET.Members;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class HelpCommandDocumentationMapperEventHandlers
    {
        public static FrozenDictionary<Command, string> CommandDocumentation { get; private set; } = FrozenDictionary<Command, string>.Empty;
        public static FrozenDictionary<CommandParameter, string> CommandParameterDocumentation { get; private set; } = FrozenDictionary<CommandParameter, string>.Empty;
        private static readonly FrozenDictionary<ICustomAttributeProvider, string> _xmlDocumentation;
        private static readonly FrozenDictionary<string, string> _commandParameterDocumentation;

        static HelpCommandDocumentationMapperEventHandlers()
        {
            IReadOnlyDictionary<string, IDocumentationMember> documentationMembers = XmlApiDocumentation.Resolve(typeof(Program).Assembly);
            Dictionary<ICustomAttributeProvider, string> xmlDocumentation = [];
            Dictionary<string, string> commandParameterDocumentation = [];
            foreach (KeyValuePair<string, IDocumentationMember> documentationMember in documentationMembers)
            {
                xmlDocumentation[documentationMember.Value.MemberInfo!] = GetDocumentation(documentationMember.Value).Trim();
                if (documentationMember.Value is MethodMember method)
                {
                    xmlDocumentation[method.Member!] = GetDocumentation(method).Trim();
                    foreach (ParamMember parameter in method.Parameters.Values)
                    {
                        commandParameterDocumentation[$"{parameter.Member!.Member.DeclaringType!.FullName}.{parameter.Member.Member.Name}.{parameter.Member.Name}"] = GetDocumentation(parameter).Trim();
                    }
                }
            }

            _xmlDocumentation = xmlDocumentation.ToFrozenDictionary();
            _commandParameterDocumentation = commandParameterDocumentation.ToFrozenDictionary();
        }

        private static string GetDocumentation(IDocumentationTag? documentationTag)
        {
            switch (documentationTag)
            {
                case null:
                    return string.Empty;
                case SeeTag seeTag:
                    List<string> formattedAttributes = new(seeTag.Attributes.Count);
                    foreach ((string key, string value) in seeTag.Attributes)
                    {
                        if (key == "cref")
                        {
                            int index = value.LastIndexOf('.');
                            formattedAttributes.Add(index == -1 ? $"`{value}`" : $"`{value[(index + 1)..]}`");
                        }
                        else if (key == "href")
                        {
                            formattedAttributes.Add($"[{value}](<{value}>)");
                        }
                        else
                        {
                            formattedAttributes.Add(value);
                        }
                    }

                    return string.Format(CultureInfo.InvariantCulture, seeTag.Value, [.. formattedAttributes, .. seeTag.EmbeddedTags.Select(GetDocumentation)]);
                case DocumentationString documentationString:
                    return string.Format(documentationString.Value, [.. documentationString.Attributes.Select(x => ""), .. documentationString.EmbeddedTags.Select(GetDocumentation)]);
                case IDocumentationMember documentationMember:
                    return $"{GetDocumentation(documentationMember.Summary)} {GetDocumentation(documentationMember.Remarks)}";
                default:
                    return string.Empty;
            }
        }

        [DiscordEvent(DiscordIntents.Guilds)]
        public static Task OnGuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs _)
        {
            Dictionary<Command, string> commandDocumentation = [];
            Dictionary<CommandParameter, string> commandParameterDocumentation = [];
            foreach (Command command in client.GetExtension<CommandsExtension>().Commands.Values.SelectMany(GetAllCommands))
            {
                ICustomAttributeProvider? memberInfo = command.Method;
                if (memberInfo is not null)
                {
                    foreach (ParameterInfo parameter in command.Method!.GetParameters())
                    {
                        foreach (CommandParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Name == parameter.Name && _commandParameterDocumentation.TryGetValue($"{parameter.Member.DeclaringType!.FullName}.{parameter.Member.Name}.{parameter.Name}", out string? parameterDocumentation) && !string.IsNullOrWhiteSpace(parameterDocumentation))
                            {
                                commandParameterDocumentation[commandParameter] = parameterDocumentation;
                                break;
                            }
                        }
                    }
                }
                else if (command.Attributes.Count != 0)
                {
                    foreach (Type type in typeof(Program).Assembly.GetTypes())
                    {
                        if (type.GetCustomAttribute<CommandAttribute>()?.Name == command.Name)
                        {
                            memberInfo = type;
                            break;
                        }
                    }
                }

                if (memberInfo is not null && _xmlDocumentation.TryGetValue(memberInfo, out string? documentation) && !string.IsNullOrWhiteSpace(documentation))
                {
                    commandDocumentation[command] = documentation;
                }
            }

            CommandDocumentation = commandDocumentation.ToFrozenDictionary();
            CommandParameterDocumentation = commandParameterDocumentation.ToFrozenDictionary();
            return Task.CompletedTask;
        }

        private static IEnumerable<Command> GetAllCommands(Command command)
        {
            if (command.Subcommands.Count != 0)
            {
                foreach (Command subcommand in command.Subcommands.SelectMany(GetAllCommands))
                {
                    yield return subcommand;
                }
            }

            yield return command;
        }
    }
}
