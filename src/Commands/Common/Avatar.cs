using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class Avatar : BaseCommandModule
    {
        private const string WebhookMessageDescription = "The webhook to get the avatar of.";
        private const string UserMessageDescription = "The user to get the avatar of. If not specified, defaults to you.";
        private const string ImageFormatDescription = "The format to convert the image to. If not specified, defaults to `.png` or, if possible, `.gif`. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).";
        private const string ImageSizeDescription = "The size to convert the image to. If not specified, defaults to `4096`.";

        [Command("avatar"), Description("Gets the avatar of a user or webhook."), Aliases("av", "pfp", "profile_picture")]
        public Task AvatarAsync(CommandContext context, [Description(WebhookMessageDescription)] DiscordMessage webhookMessage, [Description(ImageSizeDescription)] ushort imageSize = 4096, [Description(ImageFormatDescription)] ImageFormat imageFormat = ImageFormat.Auto) => AvatarAsync(context, webhookMessage.Author, imageSize, imageFormat);

        [Command("avatar")]
        public Task AvatarAsync(CommandContext context, [Description(ImageSizeDescription)] ushort imageSize = 4096, [Description(ImageFormatDescription)] ImageFormat imageFormat = ImageFormat.Auto) => AvatarAsync(context, context.User, imageSize, imageFormat);

        [Command("avatar")]
        public Task AvatarAsync(CommandContext context, [Description(UserMessageDescription)] DiscordUser user, [Description(ImageSizeDescription)] ushort imageSize = 4096, [Description(ImageFormatDescription)] ImageFormat imageFormat = ImageFormat.Auto) => context.RespondAsync(user.GetAvatarUrl(imageFormat, imageSize));
    }
}
