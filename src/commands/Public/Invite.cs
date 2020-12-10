using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public {
    public class Invite : BaseCommandModule {

        [Command("invite")]
        [Aliases(new string[] { "link" })]
        [Description("Sends the link to add Tomoe to a guild.")]
        public async Task Mention(CommandContext context) => Tomoe.Program.SendMessage(context, "<https://discord.com/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot>");
    }
}