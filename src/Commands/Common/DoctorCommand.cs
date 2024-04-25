using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Provides information about the bot's current permissions and the permissions required for each command.
    /// </summary>
    public static class DoctorCommand
    {
        private const string AdministratorWarning = "I have the `Administrator` permission; I can execute all of my commands without issue. It is advised you re-invite me with the proper permissions - for a boost in security. The `invite` command will give you the link with the correct permissions.";
        private const string DiffExplanation = "The red permissions are the permissions that I do not have. The green permissions are the ones I do have. If a command has a red permission, that means I cannot execute it.";

        /// <summary>
        /// Helps diagnose permission issues with the bot.
        /// </summary>
        [Command("doctor"), RequireGuild]
        public static ValueTask ExecuteAsync(CommandContext context)
        {
            StringBuilder stringBuilder = new();
            Dictionary<string, string> permissionsText = [];
            DiscordPermissions botPermissions = context.Guild!.CurrentMember.Permissions;
            foreach (Command command in context.Extension.Commands.Values)
            {
                DiscordPermissions permissions = GetCommandPermissions(command);
                if (permissions == default)
                {
                    continue;
                }

                stringBuilder.Clear();
                stringBuilder.Append(command.Name);
                stringBuilder.AppendLine(": ");
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

                permissionsText.Add(command.Name, stringBuilder.ToString());
            }

            stringBuilder.Clear();
            stringBuilder.AppendLine(context.Guild.CurrentMember.Permissions.HasFlag(DiscordPermissions.Administrator) ? AdministratorWarning : DiffExplanation);
            stringBuilder.AppendLine("```diff");
            foreach ((string _, string commandPermissions) in permissionsText.OrderBy(x => x.Key))
            {
                stringBuilder.AppendLine(commandPermissions);
            }

            stringBuilder.Append("```");
            return context.RespondAsync(new DiscordEmbedBuilder()
            {
                Title = "Permissions Doctor",
                Description = stringBuilder.ToString()
            });
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
