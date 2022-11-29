using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Common
{
	public class GuildIcon : BaseCommandModule
	{
		[Command("guild_icon"), Description("Gets the guild's icon."), Aliases("server_icon"), Priority(1)]
		public Task GuildIconAsync(CommandContext context, [Description("(Optional) What size the image should be. Must be a power of two.")] ushort imageSize = 4096, [Description("(Optional) What format the image should be. See [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats).")] ImageFormat imageFormat = ImageFormat.Png)
			=> context.RespondAsync(context.Guild.GetIconUrl(imageFormat, imageSize));

		[Command("guild_icon"), Description("Gets the guild's icon."), Priority(0)]
		public async Task GuildIconAsync(CommandContext context, [Description("Which guild id to grab")] ulong guildId)
			=> await context.RespondAsync($"https://cdn.discordapp.com/icons/{guildId}/{(await context.Client.GetGuildPreviewAsync(guildId)).Icon}?size=4096");
	}
}
