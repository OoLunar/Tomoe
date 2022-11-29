using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Common
{
	/// <summary>
	/// Gets a DiscordUser's avatar, also known as a profile picture.
	/// </summary>
	public class Avatar : BaseCommandModule
	{
		[Command("avatar"), Description("Gets the profile picture of the requested user. Defaults to the requester when no user is specified."), Aliases("pfp", "profile_picture")]
		public Task AvatarAsync(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requester.")] DiscordUser? user = null, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize = 4096, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
			=> context.RespondAsync(user is null ? "User not found." : user.GetAvatarUrl(imageFormat, imageSize));
	}
}
