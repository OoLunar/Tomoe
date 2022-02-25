using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Common
{
    public class GuildProfilePicture : BaseCommandModule
    {
        [Command("guild_profile_picture"), Description("Gets the profile picture of the requested user. Defaults to the requester when no user is specified."), RequireGuild, Aliases("guild_pfp", "guild_avatar", "gpfp")]
        public async Task GuildProfilePictureAsync(CommandContext context) => await GuildProfilePictureAsync(context, context.Member, 4096, ImageFormat.Png);

        [Command("guild_profile_picture")]
        public async Task GuildProfilePictureAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requester.")] DiscordMember user, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => await GuildProfilePictureAsync(context, user, 4096, imageFormat);

        [Command("guild_profile_picture")]
        public async Task GuildProfilePictureAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requester.")] DiscordMember user, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
            => await context.RespondAsync(user is null ? "User not found." : user.GetGuildAvatarUrl(imageFormat, imageSize));
    }
}