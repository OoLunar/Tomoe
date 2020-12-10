using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public {
    public class Pfp : BaseCommandModule {

        [Command("pfp")]
        [Aliases(new string[] { "profile_picture", "avatar" })]
        [Description("Gets the profile picture of the initiator or the request user.")]
        public async Task Mention(CommandContext context) => Mention(context, context.User, 1024, ImageFormat.Png);

        [Command("pfp")]
        public async Task Mention(CommandContext context, [Description("(Optional) Gets the requested users profile picture. Defaults to the initiator.")] DiscordUser user, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => Mention(context, user, 1024, imageFormat);

        [Command("pfp")]
        public async Task Mention(CommandContext context, [Description("(Optional) Gets the request users profile picture. Defaults to the initiator.")] DiscordUser user, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => Program.SendMessage(context, user == null ? "User not found." : user.GetAvatarUrl(imageFormat, imageSize));
    }
}