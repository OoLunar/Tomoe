using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Events.Handlers;
using OoLunar.Tomoe.Interactivity;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Provides information about the bot's current permissions and the permissions required for each command.
    /// </summary>
    public static class DoctorCommand
    {
        private const string AdministratorWarning = "⚠️ I have the `Administrator` permission; I can execute all of my commands without issue. It is advised you re-invite me with the proper permissions for a boost in security. The `invite` command will give you the link with the correct permissions. ⚠️";
        private const string MissingRequiredPermissionsWarning = "❌ The following permissions are required for most commands to work properly: `Create Embeds`, `Send Messages`, `Send Thread Messages` and `View Channels`. Please re-invite me with the proper permissions. The `invite` command will give you the link with the correct permissions. ❌";
        private const string DiffExplanation = "The red permissions are the permissions that I do not have. The green permissions are the ones I do have. If a command has a red permission, that means I cannot execute it.";
        private static readonly DiscordEmoji SuccessEmoji = DiscordEmoji.FromUnicode("✅");
        private static readonly DiscordEmoji FailureEmoji = DiscordEmoji.FromUnicode("❌");

        /// <summary>
        /// Helps diagnose permission issues with the bot.
        /// </summary>
        [Command("doctor"), RequireGuild]
        public static async ValueTask ExecuteAsync(CommandContext context)
        {
            DiscordMessageBuilder messageBuilder = new();
            if (context.Guild!.CurrentMember.Permissions.HasPermission(DiscordPermission.Administrator))
            {
                messageBuilder.Content = AdministratorWarning;
            }
            else if (!context.Guild.CurrentMember.Permissions.HasAllPermissions(DiscordPermission.EmbedLinks, DiscordPermission.SendMessages, DiscordPermission.SendThreadMessages, DiscordPermission.ViewChannel))
            {
                messageBuilder.Content = MissingRequiredPermissionsWarning;
            }

            List<Page> pages = [];
            DiscordPermissions allRequiredPermissions = DiscordPermissions.None;
            foreach (Command command in context.Extension.Commands.Values.OrderBy(x => x.Name))
            {
                DiscordPermissions permissions = GetCommandPermissions(command);
                if (permissions == DiscordPermissions.None)
                {
                    continue;
                }

                DiscordEmbedBuilder embedBuilder = new()
                {
                    Title = $"Permissions Doctor - {command.Name.Titleize()}",
                    Description = HelpCommandDocumentationMapperEventHandler.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation : "No description provided.",
                    Color = new(0x6b73db),
                    Footer = new()
                    {
                        Text = DiffExplanation
                    }
                };

                allRequiredPermissions |= permissions;
                embedBuilder.AddField("Permissions", GenerateDiff(permissions, context.Guild.CurrentMember.Permissions, out bool missingPermissions));
                pages.Add(new Page(
                    new DiscordMessageBuilder(messageBuilder).AddEmbed(embedBuilder),
                    command.Name.Titleize(),
                    missingPermissions ? "This command is missing required permissions." : "All required permissions are granted!",
                    missingPermissions ? FailureEmoji : SuccessEmoji)
                );
            }

            if (allRequiredPermissions != DiscordPermissions.None)
            {
                DiscordEmbedBuilder embedBuilder = new()
                {
                    Title = "All Required Permissions",
                    Description = "These are the permissions required for all commands to work properly.",
                    Color = new(0x6b73db),
                    Footer = new()
                    {
                        Text = DiffExplanation
                    }
                };

                embedBuilder.AddField("Permissions", GenerateDiff(allRequiredPermissions, context.Guild.CurrentMember.Permissions, out bool missingPermissions));
                pages.Insert(0, new Page(
                    new DiscordMessageBuilder(messageBuilder).AddEmbed(embedBuilder),
                    "All Required Permissions",
                    missingPermissions ? "These permissions are missing." : "All required permissions are granted!",
                    missingPermissions ? FailureEmoji : SuccessEmoji)
                );
            }

            DiscordPermissions channelPermissions = context.Channel.PermissionsFor(context.Guild.CurrentMember);
            if (context is not TextCommandContext textCommandContext || channelPermissions.HasPermission(DiscordPermission.SendMessages))
            {
                await context.PaginateAsync(pages);
                return;
            }
            else if (!channelPermissions.HasFlag(DiscordPermission.SendMessages))
            {
                try
                {
                    // Try to DM the user the embed
                    await context.Member!.PaginateAsync(context.ServiceProvider.GetRequiredService<Procrastinator>(), pages);
                }
                catch (DiscordException)
                {
                    // Try to react to the message
                    if (channelPermissions.HasFlag(DiscordPermission.AddReactions))
                    {
                        try
                        {
                            await textCommandContext.Message.CreateReactionAsync(FailureEmoji);
                        }
                        catch (DiscordException) { }
                    }
                }
            }
            else if (!channelPermissions.HasFlag(DiscordPermission.EmbedLinks))
            {
                messageBuilder.Content += "\n❌ This command requires the `Embed Links` permission to function. ❌";
                await context.RespondAsync(messageBuilder);
            }

            throw new UnreachableException("Something went wrong and now I don't know what to do. Try executing this command as a slash command?");
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

        private static string GenerateDiff(DiscordPermissions permissions, DiscordPermissions currentPermissions, out bool missingPermissions)
        {
            missingPermissions = false;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("```diff");
            foreach (DiscordPermission permission in DiscordPermissions.All.EnumeratePermissions().OrderBy(permission => permission.ToStringFast()))
            {
                if (!permissions.HasFlag(permission))
                {
                    continue;
                }

                if (currentPermissions.HasFlag(permission))
                {
                    stringBuilder.Append("+ ");
                }
                else
                {
                    missingPermissions = true;
                    stringBuilder.Append("- ");
                }

                stringBuilder.AppendLine(permission.ToStringFast());
            }

            stringBuilder.AppendLine("```");
            return stringBuilder.ToString();
        }
    }
}
