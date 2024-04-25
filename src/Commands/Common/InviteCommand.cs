using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class InviteCommand
    {
        [Command("invite"), Description("Sends the link to add Tomoe to a guild without an embed.")]
        public static ValueTask InviteAsync(CommandContext context)
        {
            DiscordPermissions requiredPermissions = GetSubcommandsPermissions(context.Extension.Commands.Values);
            StringBuilder stringBuilder = new();
            stringBuilder.Append("<https://discord.com/api/oauth2/authorize?client_id=");
            stringBuilder.Append(context.Client.CurrentUser.Id);
            stringBuilder.Append("&scope=bot%20applications.commands");
            if (requiredPermissions != 0)
            {
                stringBuilder.Append("&permissions=");
                stringBuilder.Append((long)requiredPermissions);
            }

            stringBuilder.Append('>');
            return context.RespondAsync(stringBuilder.ToString());
        }

        private static DiscordPermissions GetSubcommandsPermissions(IEnumerable<Command> subCommands)
        {
            DiscordPermissions permissions = 0;
            foreach (Command subCommand in subCommands)
            {
                if (subCommand.Subcommands.Count != 0)
                {
                    permissions |= GetSubcommandsPermissions(subCommand.Subcommands);
                }

                if (subCommand.Attributes.OfType<RequirePermissionsAttribute>().FirstOrDefault() is RequirePermissionsAttribute permissionsAttribute)
                {
                    permissions |= permissionsAttribute.BotPermissions;
                }
            }

            return permissions;
        }
    }
}
