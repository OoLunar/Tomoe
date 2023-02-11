using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class TimeOfCommand : BaseCommand
    {
        private static readonly DiscordMessageArgumentConverter _discordMessageArgumentConverter = new();
        private static readonly UInt64ArgumentConverter _uint64ArgumentConverter = new();

        [Command("time_of", "when_was")]
        public static async Task ExecuteAsync(CommandContext context, params string[] messages)
        {
            List<ulong> messageIds = new();
            StringBuilder invalidMessageIds = new();
            foreach (string message in messages)
            {
                Optional<ulong> parsedMessageId = await _uint64ArgumentConverter.ConvertAsync(context, null!, message);
                if (parsedMessageId.HasValue)
                {
                    messageIds.Add(parsedMessageId.Value);
                    continue;
                }

                Optional<DiscordMessage> parsedMessage = await _discordMessageArgumentConverter.ConvertAsync(context, null!, message);
                if (parsedMessage.HasValue)
                {
                    messageIds.Add(parsedMessage.Value.Id);
                    continue;
                }

                invalidMessageIds.AppendFormat("`{0}`\n", message);
            }

            if (invalidMessageIds.Length != 0)
            {
                await context.ReplyAsync($"Invalid message ids or links: {invalidMessageIds}");
                return;
            }

            messageIds.Sort();
            StringBuilder timestamps = new();
            foreach (ulong messageId in messageIds)
            {
                timestamps.Append(CultureInfo.InvariantCulture, $"{Formatter.InlineCode(messageId.ToString(CultureInfo.InvariantCulture))} => {Formatter.InlineCode(messageId.GetSnowflakeTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffff", CultureInfo.InvariantCulture))}\n");
            }

            await context.ReplyAsync(timestamps.ToString());
        }
    }
}
