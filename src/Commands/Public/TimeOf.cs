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

namespace Tomoe.Commands.Public
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
                timestamps.Append($"{Formatter.InlineCode(messages[i].ToString(CultureInfo.InvariantCulture))} => {Formatter.InlineCode(messages[i].GetSnowflakeTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffff", CultureInfo.InvariantCulture))}\n");
            }

            if (messages.Length > 10)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Timestamps for {messages.Length} messages!");
                InteractivityExtension interactivity = context.Client.GetInteractivity();
                Page[] pages = interactivity.GeneratePagesInEmbed(timestamps.ToString(), SplitType.Line, embedBuilder).ToArray();

                if (pages.Length == 1)
                {
                    await Program.SendMessage(context, null, pages[0].Embed);
                }
                else
                {
                    await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
                }
            }
            else
            {
                await Program.SendMessage(context, timestamps.ToString());
            }
        }

        [Command("time_of")]
        public async Task Overload(CommandContext context, params string[] messages)
        {
            List<ulong> messageIds = new();
            Dictionary<string, string> invalidMessages = new();
            foreach (string message in messages)
            {
                if (Uri.TryCreate(message, UriKind.Absolute, out Uri messageLink))
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
                await Program.SendMessage(context, $"Failed to get the time of the following messages:\n{string.Join('\n', invalidMessages.Select(pair => pair.Key + " - " + pair.Value))}");
            }
            await Overload(context, messageIds.ToArray());
        }
    }
}