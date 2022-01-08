using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class ProfilePicture : BaseCommandModule
    {
        [Command("profile_picture"), Description("Gets the profile picture of the requested user. Defaults to the requestor when no user is specified."), Aliases("pfp", "avatar")]
        public async Task ProfilePictureAsync(CommandContext context) => await ProfilePictureAsync(context, context.User, 4096, ImageFormat.Png);

        [Command("profile_picture")]
        public async Task ProfilePictureAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => await ProfilePictureAsync(context, user, 4096, imageFormat);

        [Command("profile_picture")]
        public async Task ProfilePictureAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
            => await context.RespondAsync(user is null ? "User not found." : user.GetAvatarUrl(imageFormat, imageSize));
    }
}