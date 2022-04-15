using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Common
{
    public class GuildAvatar : BaseCommandModule
    {
        [Command("guild_avatar"), Description("Gets the profile picture of the requested user. Defaults to the requester when no user is specified."), RequireGuild, Aliases("guild_pfp", "guild_profile_picture", "gpfp")]
        public async Task GuildAvatarAsync(CommandContext context) => await GuildAvatarAsync(context, context.Member!, 4096, ImageFormat.Png);

        [Command("guild_avatar")]
        public async Task GuildAvatarAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requester.")] DiscordMember member, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => await GuildAvatarAsync(context, member, 4096, imageFormat);

        [Command("guild_avatar")]
        public async Task GuildAvatarAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requester.")] DiscordMember member, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
            => await context.RespondAsync(member is null ? "User not found." : member.GetGuildAvatarUrl(imageFormat, imageSize));
    }
}
