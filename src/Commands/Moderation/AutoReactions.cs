using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("auto_react"), Description("Reacts automatically when a new message is posted."), Aliases("autoreact", "ar"), RequireUserPermissions(Permissions.ManageMessages)]
	public class AutoReactions : BaseCommandModule
	{
		public Database Database { private get; set; }

		[GroupCommand]
		public async Task Overload(CommandContext context, [Description("The channel to react in.")] DiscordChannel channel, [Description("The emoji to react with.")] DiscordEmoji emoji)
		{
			AutoReaction autoReaction = Database.AutoReactions.FirstOrDefault(autoReaction => autoReaction.GuildId == context.Guild.Id && autoReaction.ChannelId == channel.Id && autoReaction.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
			if (autoReaction != null)
			{
				_ = await Program.SendMessage(context, $"Auto reaction {emoji} on {channel.Mention} already exists!");
				return;
			}

			autoReaction = new();
			autoReaction.GuildId = context.Guild.Id;
			autoReaction.ChannelId = channel.Id;
			autoReaction.EmojiName = emoji.GetDiscordName();
			_ = Database.AutoReactions.Add(autoReaction);
			_ = await Database.SaveChangesAsync();
			await ModLogs.Record(context, "Autoreaction Create", $"{context.User.Mention} has created an autoreaction with emoji {emoji} on channel {channel.Mention}.");
			_ = await Program.SendMessage(context, $"From here on out, every message in {channel.Mention} will have the {emoji} reaction added to it!");
		}

		[Command("list"), Description("Shows all the current auto reactions for a channel."), Aliases("show", "ls")]
		public async Task List(CommandContext context, [Description("Lists all autoreactions for the specified channel.")] DiscordChannel channel)
		{
			StringBuilder stringBuilder = new();
			foreach (AutoReaction autoReaction in Database.AutoReactions.Where(autoReaction => autoReaction.ChannelId == channel.Id && autoReaction.GuildId == context.Guild.Id))
			{
				_ = stringBuilder.AppendLine($"{DiscordEmoji.FromName(context.Client, autoReaction.EmojiName, true)}");
			}

			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Autoreactions in channel {channel.Mention}");
			if (stringBuilder.Length <= 2000)
			{
				embedBuilder.Description = stringBuilder.ToString();
				_ = await Program.SendMessage(context, null, embedBuilder.Build());
			}
			else
			{
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(stringBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedBuilder);
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
		}

		[Command("remove"), Description("Removes an autoreaction from a channel."), Aliases("rm", "delete", "del")]
		public async Task Remove(CommandContext context, [Description("The channel to remove the autoreaction from.")] DiscordChannel channel, [Description("The emoji to stop autoreacting with.")] DiscordEmoji emoji)
		{
			AutoReaction autoReaction = Database.AutoReactions.FirstOrDefault(autoReaction => autoReaction.GuildId == context.Guild.Id && autoReaction.ChannelId == channel.Id && autoReaction.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
			if (autoReaction != null)
			{
				_ = Database.AutoReactions.Remove(autoReaction);
				_ = await Database.SaveChangesAsync();
				await ModLogs.Record(context, "Autoreaction Remove", $"{context.User.Mention} has removed an autoreaction with emoji {emoji} on channel {channel.Mention}.");
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
