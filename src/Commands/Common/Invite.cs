using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class Invite : BaseCommandModule
    {
        [Command("invite"), Description("Sends the link to add Tomoe to a guild without an embed.")]
        public async Task InviteAsync(CommandContext context) => await context.RespondAsync(Formatter.EmbedlessUrl(new($"https://discord.com/api/oauth2/authorize?client_id={context.Client.CurrentUser.Id}&permissions=8&scope=bot")));
    }
}