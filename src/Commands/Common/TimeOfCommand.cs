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

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class TimeOfCommand : BaseCommandModule
    {
        [Command("time_of"), Description("Gets the time of the messages linked."), Aliases("when_was", "timestamp")]
        public Task TimeOfAsync(CommandContext context) => context.Message.ReferencedMessage == null
            ? context.RespondAsync("Please respond to a message or provide message links.")
            : context.RespondAsync(Formatter.InlineCode(context.Message.ReferencedMessage.Id.GetSnowflakeTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'ffff", CultureInfo.InvariantCulture)));

        [Command("time_of")]
        public Task TimeOfAsync(CommandContext context, [Description("A list of links that go to a Discord message.")] params DiscordMessage[] messages) => TimeOfAsync(context, messages.Select(message => message.Id).ToArray());

        [Command("time_of")]
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
    }
}
