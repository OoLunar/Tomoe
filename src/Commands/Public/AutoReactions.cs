using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Public
{
	[Group("auto_react"), Description("Reacts automatically when a new message is posted."), Aliases("autoreact", "ar"), RequireUserPermissions(Permissions.ManageMessages)]
	public class AutoReactions : BaseCommandModule
	{
		[GroupCommand]
		public async Task Overload(CommandContext context, DiscordChannel channel, DiscordEmoji emoji)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			AutoReaction autoReaction = database.AutoReactions.FirstOrDefault(autoReaction => autoReaction.GuildId == context.Guild.Id && autoReaction.ChannelId == channel.Id && autoReaction.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
			if (autoReaction != null)
			{
				_ = await Program.SendMessage(context, $"Auto reaction {emoji} on {channel.Mention} already exists!");
				return;
			}

			autoReaction = new();
			autoReaction.GuildId = context.Guild.Id;
			autoReaction.ChannelId = channel.Id;
			autoReaction.EmojiName = emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString();
			_ = database.AutoReactions.Add(autoReaction);
			_ = await database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"From here on out, every message in {channel.Mention} will have the {emoji} reaction added to it!");
		}

		[Command("remove"), Description("Removes an autoreaction from a channel."), Aliases("rm", "delete", "del")]
		public async Task Remove(CommandContext context, DiscordChannel channel, DiscordEmoji emoji)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			AutoReaction autoReaction = database.AutoReactions.FirstOrDefault(autoReaction => autoReaction.GuildId == context.Guild.Id && autoReaction.ChannelId == channel.Id && autoReaction.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
			if (autoReaction != null)
			{
				_ = database.AutoReactions.Remove(autoReaction);
				_ = await database.SaveChangesAsync();
				_ = await Program.SendMessage(context, $"Auto reaction {emoji} on {channel.Mention} has been removed!");
				return;
			}
			else
			{
				_ = await Program.SendMessage(context, Formatter.Bold("[Error]: Autoreaction doesn't exist!"));
			}
		}
	}
}
