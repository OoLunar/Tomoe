using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class TimeOf : BaseCommandModule
    {
        [Command("time_of"), Description("Gets the time of the messages linked."), Aliases("when_was", "timestamp")]
        public async Task Overload(CommandContext context, params ulong[] messages)
        {
            messages = messages.Distinct().OrderBy(snowflake => snowflake).ToArray();
            StringBuilder timestamps = new();
            for (int i = 0; i < messages.Length; i++)
            {
                timestamps.Append(CultureInfo.InvariantCulture, $"{Formatter.InlineCode(messages[i].ToString(CultureInfo.InvariantCulture))} => {Formatter.InlineCode(messages[i].GetSnowflakeTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffff", CultureInfo.InvariantCulture))}\n");
            }

            if (messages.Length > 10)
            {
                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.Title = $"Timestamps for {messages.Length} messages!";
                embedBuilder.Color = new DiscordColor("#7b84d1");
                embedBuilder.Author = new()
                {
                    Name = context.Guild == null ? context.User.Username : context.Member.DisplayName,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                };

                InteractivityExtension interactivity = context.Client.GetInteractivity();
                Page[] pages = interactivity.GeneratePagesInEmbed(timestamps.ToString(), SplitType.Line, embedBuilder).ToArray();

                if (pages.Length == 1)
                {
                    await context.RespondAsync(null, pages[0].Embed);
                }
                else
                {
                    await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
                }
            }
            else
            {
                await context.RespondAsync(timestamps.ToString());
            }
        }

        [Command("time_of")]
        public async Task Overload(CommandContext context, params string[] messages)
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

            if (invalidMessages.Any())
            {
                await context.RespondAsync($"Failed to get the time of the following messages:\n{string.Join('\n', invalidMessages.Select(pair => pair.Key + " - " + pair.Value))}");
            }
            await Overload(context, messageIds.ToArray());
        }
    }
}