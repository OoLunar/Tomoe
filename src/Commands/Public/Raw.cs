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
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = "Message link isn't from Discord!",
                        IsEphemeral = true
                    });
                    return;
                }
                else if (messageLink.Segments.Length != 5 || messageLink.Segments[1] != "channels/" || (ulong.TryParse(messageLink.Segments[2].Remove(messageLink.Segments[2].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong guildId) && guildId != context.Guild.Id))
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = "Message link isn't from this guild!",
                        IsEphemeral = true
                    });
                    return;
                }
                else if (ulong.TryParse(messageLink.Segments[3].Remove(messageLink.Segments[3].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong channelId))
                {
                    channel = context.Guild.GetChannel(channelId);
                    if (channel == null)
                    {
                        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            Content = $"Unknown channel <#{channelId}> ({channelId})",
                            IsEphemeral = true
                        });
                    }
                    else if (ulong.TryParse(messageLink.Segments[4], NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageLinkId))
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
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"{messageString} is not a message id or link!",
                    IsEphemeral = true
                });
                return;
            }

            try
            {
                message = await channel.GetMessageAsync(messageId);
            }
            catch (NotFoundException)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "Message not found! Did you call the command in the correct channel?",
                    IsEphemeral = true
                });
                return;
            }
            catch (UnauthorizedException)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "I don't have access to that message. Please fix my Discord permissions!",
                    IsEphemeral = true
                });
                return;
            }

            if (message.Content == string.Empty && message.Embeds.Any())
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = Constants.RawEmbed,
                    IsEphemeral = true
                });
            }
            else
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = Formatter.Sanitize(message.Content)
                });
            }
        }
    }
}
