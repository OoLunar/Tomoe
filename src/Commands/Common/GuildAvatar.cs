using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class GuildAvatar : BaseCommandModule
    {
        private const string MemberMessageDescription = "The user's pfp to be shown. Defaults to the requester.";
        private const string ImageSizeDescription = "The size to convert the image to. If not specified, defaults to `4096`.";
        private const string ImageFormatDescription = "The format to convert the image to. If not specified, defaults to `.png` or, if possible, `.gif`. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).";

        [Command("guild_avatar"), Description("Gets the guild avatar of the requested user. Defaults to you when no user is specified."), RequireGuild, Aliases("guild_pfp", "guild_profile_picture", "gpfp")]
        public Task GuildAvatarAsync(CommandContext context, [Description(MemberMessageDescription)] DiscordMember? member = null, [Description(ImageSizeDescription)] ushort imageSize = 4096, [Description(ImageFormatDescription)] ImageFormat imageFormat = ImageFormat.Auto)
            => context.RespondAsync((member ?? context.Member!).GetGuildAvatarUrl(imageFormat, imageSize));
    }
}
