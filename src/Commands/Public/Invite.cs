using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Tomoe.Commands.Public
{
    public class Invite : BaseCommandModule
    {
        [Command("invite"), Description("Sends the link to add Tomoe to a guild without an embed."), Aliases("link")]
        public async Task Overload(CommandContext context) => await Program.SendMessage(context, Formatter.EmbedlessUrl(new($"https://discord.com/api/oauth2/authorize?client_id={context.Client.CurrentUser.Id}&permissions=8&scope=bot")));
    }
}