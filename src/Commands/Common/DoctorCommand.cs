using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using OoLunar.Tomoe.Events.Handlers;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Provides information about the bot's current permissions and the permissions required for each command.
    /// </summary>
    public static class DoctorCommand
    {
        private const string AdministratorWarning = "⚠️ I have the `Administrator` permission; I can execute all of my commands without issue. It is advised you re-invite me with the proper permissions - for a boost in security. The `invite` command will give you the link with the correct permissions. ⚠️";
        private const string MissingRequiredPermissionsWarning = "❌ The following permissions are required for all commands to work properly: `Send Messages`, `Send Messages in Threads`, and `Access Channels`. Please re-invite me with the proper permissions. The `invite` command will give you the link with the correct permissions. ❌";
        private const string DiffExplanation = "The red permissions are the permissions that I do not have. The green permissions are the ones I do have. If a command has a red permission, that means I cannot execute it.";

        /// <summary>
        /// Helps diagnose permission issues with the bot.
        /// </summary>
        [Command("doctor"), RequireGuild, RequirePermissions(DiscordPermissions.AccessChannels | DiscordPermissions.SendMessages | DiscordPermissions.SendMessagesInThreads | DiscordPermissions.EmbedLinks, DiscordPermissions.None)]
        public static async ValueTask ExecuteAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Permissions Doctor",
                Footer = new()
                {
                    Text = DiffExplanation
                }
            };

            DiscordPermissions botPermissions = context.Guild!.CurrentMember.Permissions;
            foreach (Command command in context.Extension.Commands.Values.OrderBy(x => x.Name))
            {
                DiscordPermissions permissions = GetCommandPermissions(command);
                if (permissions == default)
                {
                    continue;
                }

                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine(HelpCommandDocumentationMapperEventHandlers.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation : "No description provided.");
                stringBuilder.AppendLine("```diff");
                for (ulong i = 0; i < (sizeof(ulong) * 8); i++)
                {
                    DiscordPermissions permission = (DiscordPermissions)Math.Pow(2, i);
                    if (!permissions.HasFlag(permission))
                    {
                        continue;
                    }
                    else if (botPermissions.HasFlag(permission))
                    {
                        stringBuilder.Append("+ ");
                    }
                    else
                    {
                        stringBuilder.Append("- ");
                    }

                    stringBuilder.AppendLine(permission.Humanize(LetterCasing.Title));
                }

                stringBuilder.AppendLine("```");
                embedBuilder.AddField(command.Name.Titleize(), stringBuilder.ToString());
            }

            if (context.Guild.CurrentMember.Permissions.HasFlag(DiscordPermissions.Administrator))
            {
                embedBuilder.WithDescription(AdministratorWarning);
            }
            else if (!botPermissions.HasFlag(DiscordPermissions.SendMessages) || !botPermissions.HasFlag(DiscordPermissions.SendMessagesInThreads) || !botPermissions.HasFlag(DiscordPermissions.AccessChannels))
            {
                embedBuilder.WithDescription(MissingRequiredPermissionsWarning);
            }

            DiscordPermissions channelPermissions = context.Channel.PermissionsFor(context.Guild.CurrentMember);
            if (context is TextCommandContext textCommandContext)
            {
                if (!channelPermissions.HasFlag(DiscordPermissions.SendMessages))
                {
                    try
                    {
                        // Try to DM the user the embed
                        await context.Member!.SendMessageAsync(embedBuilder);
                    }
                    catch (DiscordException)
                    {
                        // Try to react to the message
                        if (channelPermissions.HasFlag(DiscordPermissions.AddReactions))
                        {
                            try
                            {
                                await textCommandContext.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":x:"));
                            }
                            catch (DiscordException) { }
                        }
                    }

                    return;
                }
                else if (!channelPermissions.HasFlag(DiscordPermissions.EmbedLinks))
                {
                    embedBuilder.WithDescription("❌ This command requires the `Embed Links` permission to function. ❌");
                    await context.RespondAsync(embedBuilder);
                    return;
                }
            }

            await context.RespondAsync(embedBuilder);
        }

        private static DiscordPermissions GetCommandPermissions(Command command)
        {
            DiscordPermissions permissions = DiscordPermissions.None;
            foreach (Command subcommand in command.Subcommands)
            {
                permissions |= GetCommandPermissions(subcommand);
            }

            if (command.Attributes.FirstOrDefault(x => x is RequirePermissionsAttribute) is RequirePermissionsAttribute attribute)
            {
                permissions |= attribute.BotPermissions | attribute.UserPermissions;
            }

            return permissions;
        }
    }
}
