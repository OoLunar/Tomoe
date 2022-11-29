using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Common
{
	public class WebhookAvatar : BaseCommandModule
	{
		[Command("webhook_avatar"), Description("Gets the profile picture of the requested webhook."), Aliases("wpfp", "whpfp", "webhook_profile_picture")]
		public Task WebhookAvatarAsync(CommandContext context, [Description("The webhook's avatar to be shown.")] DiscordMessage message, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize = 4096, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
			=> context.RespondAsync(!message.WebhookMessage ? "Not a webhook message." : message.Author.GetAvatarUrl(imageFormat, imageSize));
	}
}
