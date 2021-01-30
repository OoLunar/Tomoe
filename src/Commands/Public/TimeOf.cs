using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class TimeOf : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.TimeOf");

		[Command("timeof"), Description("Gets the time of the messages linked."), Aliases("time_of", "whenwas", "when_was", "timestamp")]
		public async Task Overload(CommandContext context, params ulong[] messages)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			await context.Channel.TriggerTypingAsync();
			messages = messages.Distinct().OrderBy(snowflake => snowflake).ToArray();
			StringBuilder timestamps = new();
			for (int i = 0; i < messages.Length; i++) _ = timestamps.Append($"{Formatter.InlineCode(messages[i].ToString(CultureInfo.InvariantCulture))} => {Formatter.InlineCode(messages[i].GetSnowflakeTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffff", CultureInfo.InvariantCulture))}\n");

			if (messages.Length > 10)
			{
				DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Timestamps for {messages.Length} messages!");
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				Page[] pages = interactivity.GeneratePagesInEmbed(timestamps.ToString(), SplitType.Line, embedBuilder).ToArray();

				if (pages.Length == 1) _ = await Program.SendMessage(context, null, pages[0].Embed);
				else await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
			else _ = await Program.SendMessage(context, timestamps.ToString());
			_logger.Trace("Message sent!");
		}
	}
}
