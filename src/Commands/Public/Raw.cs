namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class Raw : SlashCommandModule
    {
        [SlashCommand("raw", "Gets the raw version of the message provided.")]
        public async Task Overload(InteractionContext context, [Option("Message", "The message id or link.")] string messageString)
        {
            DiscordMessage message = null;
            DiscordChannel channel = null;
            ulong messageId = 0;
            if (Uri.TryCreate(messageString, UriKind.Absolute, out Uri messageLink))
            {
                if (messageLink.Host != "discord.com" && messageLink.Host != "discordapp.com")
                {
                    //TODO: Make a web request and try escaping the content
                    await Program.SendMessage(context, "Message link isn't from Discord!");
                    return;
                }
                else if (messageLink.Segments.Length != 5 || messageLink.Segments[1] != "channels/" || (ulong.TryParse(messageLink.Segments[2].Remove(messageLink.Segments[2].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong guildId) && guildId != context.Guild.Id))
                {
                    await Program.SendMessage(context, "Message link isn't from this guild!");
                    return;
                }
                else if (ulong.TryParse(messageLink.Segments[3].Remove(messageLink.Segments[3].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong channelId))
                {
                    channel = context.Guild.GetChannel(channelId);
                    if (channel == null)
                    {
                        await Program.SendMessage(context, $"Unknown channel <#{channelId}> ({channelId})");
                        return;
                    }

                    if (ulong.TryParse(messageLink.Segments[4], NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageLinkId))
                    {

                        messageId = messageLinkId;
                    }
                }
            }
            else if (ulong.TryParse(messageString, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageIdArgs))
            {
                channel = context.Channel;
                messageId = messageIdArgs;
            }
            else
            {
                await Program.SendMessage(context, $"{messageString} is not a message id or link!");
                return;
            }

            try
            {
                message = await channel.GetMessageAsync(messageId);
            }
            catch (NotFoundException)
            {
                await Program.SendMessage(context, "Message not found! Did you call the command in the correct channel?");
                return;
            }
            catch (UnauthorizedException)
            {
                await Program.SendMessage(context, "I don't have access to the message! Please fix my Discord permissions!");
                return;
            }

            if (message.Content == string.Empty && message.Embeds.Any())
            {
                await Program.SendMessage(context, Constants.RawEmbed);
            }
            else
            {
                await Program.SendMessage(context, Formatter.Sanitize(message.Content));
            }
        }
    }
}
