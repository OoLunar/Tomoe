using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class InviteCommand : BaseCommandModule
    {
        [Command("invite"), Description("Sends the link to add Tomoe to a guild without an embed.")]
        public Task InviteAsync(CommandContext context)
        {
            Permissions requiredPermissions = 0;
            foreach (RequirePermissionsAttribute requirePermissionsAttribute in typeof(Program).Assembly.DefinedTypes.SelectMany(x => x.GetMethods()).SelectMany(x => x.GetCustomAttributes(typeof(RequirePermissionsAttribute), true).OfType<RequirePermissionsAttribute>()))
            {
                requiredPermissions |= requirePermissionsAttribute.Permissions;
            }

            foreach (RequireBotPermissionsAttribute requireBotPermissionsAttribute in typeof(Program).Assembly.DefinedTypes.SelectMany(x => x.GetMethods()).SelectMany(x => x.GetCustomAttributes(typeof(RequireBotPermissionsAttribute), true).OfType<RequireBotPermissionsAttribute>()))
            {
                requiredPermissions |= requireBotPermissionsAttribute.Permissions;
            }

            foreach (RequirePermissionsAttribute requirePermissionsAttribute in typeof(Program).Assembly.DefinedTypes.SelectMany(x => x.GetCustomAttributes(typeof(RequirePermissionsAttribute), true).OfType<RequirePermissionsAttribute>()))
            {
                requiredPermissions |= requirePermissionsAttribute.Permissions;
            }

            foreach (RequireBotPermissionsAttribute requireBotPermissionsAttribute in typeof(Program).Assembly.DefinedTypes.SelectMany(x => x.GetCustomAttributes(typeof(RequireBotPermissionsAttribute), true).OfType<RequireBotPermissionsAttribute>()))
            {
                requiredPermissions |= requireBotPermissionsAttribute.Permissions;
            }

            return context.RespondAsync(Formatter.EmbedlessUrl(new($"https://discord.com/api/oauth2/authorize?client_id={context.Client.CurrentUser.Id}&scope=bot{(requiredPermissions == 0 ? "" : $"&permissions={(long)requiredPermissions}")}")));
        }
    }
}
