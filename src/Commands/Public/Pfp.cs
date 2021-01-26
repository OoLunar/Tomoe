using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class ProfilePicture : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.ProfilePicture");
		[Command("profilepicture"), Description("Gets the profile picture of the requested user. Defaults to the requestor when no user is specified."), Aliases("profile_picture", "pfp", "avatar")]
		public async Task Mention(CommandContext context) => Mention(context, context.User, 1024, ImageFormat.Png);

		[Command("profilepicture")]
		public async Task Mention(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => Mention(context, user, 1024, imageFormat);

		[Command("profilepicture")]
		public async Task Mention(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			bool userExists = user != null;
			_logger.Trace($"User exist: {userExists}");
			if (userExists)
			{
				_logger.Trace($"Getting {user.Username}'s profile picture in {imageFormat} form and {imageSize}x{imageSize} resolution.");
				string userAvatarUrl = user.GetAvatarUrl(imageFormat, imageSize);
				_logger.Trace($"{user.Username}'s profile picture: {userAvatarUrl}");
				_ = Program.SendMessage(context, userAvatarUrl);
				_logger.Trace("Message sent!");
			}
			else
			{
				_ = Program.SendMessage(context, Formatter.Bold("[Error: User not found.]"));
				_logger.Trace("Message sent!");
			}
		}
	}
}
