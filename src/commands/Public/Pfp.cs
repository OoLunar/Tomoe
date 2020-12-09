using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public {
    public class Pfp : BaseCommandModule {

        [Command("pfp")]
        [Aliases(new string[] { "profile_picture", "avatar" })]
        [Description("Gets the profile picture of the initiator or the request user.")]
        public async Task Mention(CommandContext context, [Description("(Optional) Gets the request users profile picture. Defaults to the initiator.")] DiscordUser victim) => Tomoe.Program.SendMessage(context, victim == null ? context.User.GetAvatarUrl(DSharpPlus.ImageFormat.Png) : victim.GetAvatarUrl(DSharpPlus.ImageFormat.Png));

        [Command("pfp")]
        [Aliases(new string[] { "profile_picture", "avatar" })]
        public async Task Mention(CommandContext context) => Mention(context, context.User);
    }
}