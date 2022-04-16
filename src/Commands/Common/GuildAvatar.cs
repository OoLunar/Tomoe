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
        public Task GuildAvatarAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requester.")] DiscordMember? member = null, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize = 4096, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
            => context.RespondAsync(member is null ? "User not found." : member.GetGuildAvatarUrl(imageFormat, imageSize));
    }
}
