namespace Tomoe.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using DSharpPlus.SlashCommands;

    public partial class Public : ApplicationCommandModule
    {
        private static HttpClient HttpClient { get; set; } = new();

        [SlashCommand("raw", "Gets the raw version of the message provided.")]
        public static async Task Raw(InteractionContext context, [Option("Message", "The message id or link.")] string messageString)
        {
            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new());

            Dictionary<string, Stream> messageFiles = new();

            DiscordMessage message;
            DiscordChannel channel = null;
            ulong messageId = 0;
            if (Uri.TryCreate(messageString, UriKind.Absolute, out Uri messageLink))
            {
                if (messageLink.Host is not "discord.com" and not "discordapp.com")
                {
                    HttpResponseMessage httpResponseMessage = await HttpClient.GetAsync(messageLink);
                    if (httpResponseMessage.IsSuccessStatusCode && httpResponseMessage.TrailingHeaders.TryGetValues("Content-Type", out IEnumerable<string> contentType))
                    {
                        if (contentType.First() == "text/plain")
                        {
                            string sanitizedWebContent = Formatter.Sanitize(await httpResponseMessage.Content.ReadAsStringAsync());
                            if (sanitizedWebContent.Length < 8000000)
                            {
                                await context.EditResponseAsync(new()
                                {
                                    Content = "Error: Output is greater than 8MB. Cannot upload as a message or file."
                                });
                                return;
                            }
                            else
                            {
                                messageFiles.Add($"{messageLink.Host}.txt", new MemoryStream(Encoding.UTF8.GetBytes(sanitizedWebContent)));
                            }
                        }
                        else
                        {
                            await context.EditResponseAsync(new()
                            {
                                Content = "Error: Content Type is not plain text, will not continue."
                            });
                            return;
                        }
                    }
                    else
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = "Error: Web request returned an unsuccessful HTTP response status code."
                        });
                        return;
                    }
                }
                else if (messageLink.Segments.Length != 5 || messageLink.Segments[1] != "channels/" || (ulong.TryParse(messageLink.Segments[2].Remove(messageLink.Segments[2].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong guildId) && guildId != context.Guild.Id))
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = context.Guild == null ? "Error: Message link isn't from this dm!" : "Error: Message link isn't from this guild!"
                    });
                    return;
                }
                else if (ulong.TryParse(messageLink.Segments[3].Remove(messageLink.Segments[3].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong channelId))
                {
                    channel = context.Guild.GetChannel(channelId);
                    if (channel == null)
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = $"Error: Unknown channel <#{channelId}> ({channelId})"
                        });
                        return;
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
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {messageString} is not a message id or link!"
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
                    Content = "Error: Message not found! Did you call the command in the correct channel?"
                });
                return;
            }
            catch (UnauthorizedException)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "Error: I don't have access to that message. Please fix my Discord permissions!"
                });
                return;
            }

            for (int i = 0; i < message.Embeds.Count; i++)
            {
                DiscordEmbed embed = message.Embeds[i];
                MemoryStream memoryStream = new();
                await JsonSerializer.SerializeAsync(memoryStream, embed);
                memoryStream.Position = 0;
                messageFiles.Add($"Embed {i + 1}.json", memoryStream);
            }

            string sanitizedContent = Formatter.Sanitize(message.Content);

            if (sanitizedContent.Length == 0)
            {
                DiscordWebhookBuilder discordWebhookBuilder = new();
                discordWebhookBuilder.AddFiles(messageFiles, true);
                discordWebhookBuilder.Content = "No text found, turned the embeds into JSON!";
                await context.EditResponseAsync(discordWebhookBuilder);
            }
            else if (sanitizedContent.Length < 2000)
            {
                await context.EditResponseAsync(new DiscordWebhookBuilder()
                {
                    Content = sanitizedContent
                }.AddFiles(messageFiles));
            }
            else if (sanitizedContent.Length < 80000)
            {
                messageFiles.Add("Message Content.txt", new MemoryStream(Encoding.UTF8.GetBytes(sanitizedContent)));
                await context.EditResponseAsync(new DiscordWebhookBuilder().AddFiles(messageFiles));
            }
            else
            {
                await context.EditResponseAsync(new DiscordWebhookBuilder()
                {
                    Content = "Error: Message content is greater than 8MB. Cannot upload as a message or a file."
                }.AddFiles(messageFiles));
            }
        }
    }
}
