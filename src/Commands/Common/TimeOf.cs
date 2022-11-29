using System;
using System.Collections.Generic;
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

namespace Tomoe.Commands.Common
{
	public class TimeOf : BaseCommandModule
	{
		[Command("time_of"), Description("Gets the time of the messages linked."), Aliases("when_was", "timestamp")]
		public Task TimeOfAsync(CommandContext context, [Description("Which messages to get the time of.")] params ulong[] messages)
		{
			messages = messages.Distinct().OrderBy(snowflake => snowflake).ToArray();
			StringBuilder timestamps = new();
			for (int i = 0; i < messages.Length; i++)
			{
				timestamps.Append(CultureInfo.InvariantCulture, $"{Formatter.InlineCode(messages[i].ToString(CultureInfo.InvariantCulture))} => {Formatter.InlineCode(messages[i].GetSnowflakeTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffff", CultureInfo.InvariantCulture))}\n");
			}

			if (messages.Length > 10)
			{
				DiscordEmbedBuilder embedBuilder = new()
				{
					Title = $"Timestamps for {messages.Length} messages!",
					Color = new DiscordColor("#7b84d1"),
					Author = new()
					{
						Name = context.Member?.DisplayName ?? context.User.Username,
						IconUrl = context.User.AvatarUrl,
						Url = context.User.AvatarUrl
					}
				};

				InteractivityExtension interactivity = context.Client.GetInteractivity();
				Page[] pages = interactivity.GeneratePagesInEmbed(timestamps.ToString(), SplitType.Line, embedBuilder).ToArray();

				return pages.Length == 1
					? context.RespondAsync(pages[0].Embed)
					: interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
			else
			{
				return context.RespondAsync(timestamps.ToString());
			}
		}

		[Command("time_of")]
		public Task TimeOfAsync(CommandContext context, [Description("A list of links that go to a Discord message.")] params string[] messages)
		{
			List<ulong> messageIds = new();
			Dictionary<string, string> invalidMessages = new();
			foreach (string message in messages)
			{
				if (Uri.TryCreate(message, UriKind.Absolute, out Uri? messageLink) && messageLink != null)
				{
					if (messageLink.Host is "discord.com" or "discordapp.com")
					{
						if (ulong.TryParse(messageLink.Segments.Last(), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageId))
						{
							messageIds.Add(messageId);
							continue;
						}
						invalidMessages.Add(message, "Not a Discord message link.");
						continue;
					}
					invalidMessages.Add(message, "Not a Discord link.");
					continue;
				}
				invalidMessages.Add(message, "Not a valid url.");
			}

			return invalidMessages.Count != 0
				? context.RespondAsync($"Failed to get the time of the following messages:\n{string.Join('\n', invalidMessages.Select(pair => pair.Key + " - " + pair.Value))}")
				: TimeOfAsync(context, messageIds.ToArray());
		}
	}
}