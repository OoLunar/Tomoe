using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public {
    public class Pfp : BaseCommandModule {
        private const string _COMMAND_NAME = "pfp";
        private const string _COMMAND_DESC = "Gets the profile picture of the requested user. Defaults to the requestor when no user is specified.";
        private const string _ARG_USER_DESC = "(Optional) The user's pfp to be shown. Defaults to the requestor.";
        private const string _ARG_IMAGE_FORMAT_DESC = "(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).";
        private const string _ARG_IMAGE_SIZE_DESC = "(Optional) What size the image should be. Must be a power of two.";

        private const string _ERROR_USER_NOT_FOUND = "**[Error: User not found.]**";

        [Command(_COMMAND_NAME), Description(_COMMAND_DESC)]
        [Aliases(new string[] { "profile_picture", "avatar" })]
        public async Task Mention(CommandContext context) => Mention(context, context.User, 1024, ImageFormat.Png);

        [Command(_COMMAND_NAME)]
        public async Task Mention(CommandContext context, [Description(_ARG_USER_DESC)] DiscordUser user, [Description(_ARG_IMAGE_FORMAT_DESC)] ImageFormat imageFormat = ImageFormat.Png) => Mention(context, user, 1024, imageFormat);

        [Command(_COMMAND_NAME)]
        public async Task Mention(CommandContext context, [Description(_ARG_USER_DESC)] DiscordUser user, [Description(_ARG_IMAGE_SIZE_DESC)] ushort imageSize, [Description(_ARG_IMAGE_FORMAT_DESC)] ImageFormat imageFormat = ImageFormat.Png) => Program.SendMessage(context, user == null ? _ERROR_USER_NOT_FOUND : user.GetAvatarUrl(imageFormat, imageSize));
    }
}