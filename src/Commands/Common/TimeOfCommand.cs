using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// The time it took for you to read this is the time it took for me to get a headache.
    /// </summary>
    public static class TimeOfCommand
    {
        private static readonly DiscordMessageConverter _discordMessageConverter = new();
        private static readonly UInt64Converter _uint64Converter = new();

        /// <summary>
        /// Sends the exact time a message was created.
        /// </summary>
        /// <param name="messages">The message IDs or links to get the time of. If only two messages were sent, I'll calculate the difference between the two.</param>
        [Command("time_of"), TextAlias("when_was")]
        public static async ValueTask ExecuteAsync(CommandContext context, params string[] messages)
        {
            TextConverterContext converterContext = new()
            {
                Channel = context.Channel,
                Command = context.Command,
                Extension = context.Extension,
                RawArguments = string.Join(' ', messages),
                ServiceScope = context.ServiceProvider.CreateAsyncScope(),
                Splicer = context.Extension.GetProcessor<TextCommandProcessor>().Configuration.TextArgumentSplicer,
                User = context.User
            };

            List<ulong> messageIds = [];
            StringBuilder invalidMessageIds = new();
            foreach (string message in messages)
            {
                converterContext.NextArgument();
                Optional<ulong> parsedMessageId = await _uint64Converter.ExecuteAsync(context, message);
                if (parsedMessageId.HasValue)
                {
                    messageIds.Add(parsedMessageId.Value);
                    continue;
                }

                Optional<DiscordMessage> parsedMessage = await _discordMessageConverter.ExecuteAsync(context, message);
                if (parsedMessage.HasValue)
                {
                    messageIds.Add(parsedMessage.Value.Id);
                    continue;
                }

                invalidMessageIds.AppendFormat("`{0}`\n", message);
            }

            if (invalidMessageIds.Length != 0)
            {
                await context.RespondAsync($"Invalid message ids or links: {invalidMessageIds}");
                return;
            }

            messageIds.Sort();
            StringBuilder timestamps = new();
            foreach (ulong messageId in messageIds)
            {
                timestamps.Append(CultureInfo.InvariantCulture, $"{Formatter.InlineCode(messageId.ToString(CultureInfo.InvariantCulture))} => {Formatter.InlineCode(messageId.GetSnowflakeTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffff", CultureInfo.InvariantCulture))}\n");
            }

            if (messageIds.Count == 2)
            {
                timestamps.AppendFormat(CultureInfo.InvariantCulture, "Difference: {0}", (messageIds[1].GetSnowflakeTime() - messageIds[0].GetSnowflakeTime()).Humanize(2));
            }

            await context.RespondAsync(timestamps.ToString());
        }
    }
}
