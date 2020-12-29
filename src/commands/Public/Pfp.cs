using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public
{
	public class Pfp : BaseCommandModule
	{
		[Command("pfp"), Description("Gets the profile picture of the requested user. Defaults to the requestor when no user is specified."), Aliases(new[] { "profile_picture", "avatar" })]
		public async Task Mention(CommandContext context) => Mention(context, context.User, 1024, ImageFormat.Png);

		[Command("pfp")]
		public async Task Mention(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => Mention(context, user, 1024, imageFormat);

		[Command("pfp")]
		public async Task Mention(CommandContext context, [Description("(Optional) The user's pfp to be shown. Defaults to the requestor.")] DiscordUser user, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png) => Program.SendMessage(context, user == null ? "**[Error: User not found.]**" : user.GetAvatarUrl(imageFormat, imageSize));
	}
}
