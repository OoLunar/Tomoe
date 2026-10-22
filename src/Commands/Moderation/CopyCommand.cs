using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// A copy pasta.
    /// </summary>
    public sealed class CopyCommand
    {
        /// <summary>
        /// Used to fetch attachments from Discord.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of <see cref="CopyCommand"/>.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for fetching attachments.</param>
        public CopyCommand(HttpClient httpClient) => _httpClient = httpClient;

        /// <summary>
        /// Copies a chunk of messages to a different channel.
        /// </summary>
        /// <param name="channel">Which channel to send the messages to.</param>
        /// <param name="firstMessage">The first message to copy, inclusive.</param>
        /// <param name="lastMessage">The last message to copy, inclusive. If omitted, copies through the latest message.</param>
        [Command("copy"), TextAlias("move"), Description("Copies a chunk of messages (inclusive) to a different channel."), RequirePermissions([DiscordPermission.ManageMessages, DiscordPermission.ReadMessageHistory])]
        public async ValueTask CopyAsync(CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
            => await ExecuteAsync(true, context, channel, firstMessage, lastMessage);

        /// <summary>
        /// Forwards a chunk of messages to a different channel.
        /// </summary>
        /// <param name="channel">Which channel to send the messages to.</param>
        /// <param name="firstMessage">The first message to forward, inclusive.</param>
        /// <param name="lastMessage">The last message to forward, inclusive. If omitted, forwards through the latest message.</param>
        [Command("forward"), Description("Forwards a chunk of messages (inclusive) to a different channel."), RequirePermissions([DiscordPermission.ManageMessages, DiscordPermission.ReadMessageHistory])]
        public async ValueTask ForwardAsync(CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
            => await ExecuteAsync(false, context, channel, firstMessage, lastMessage);

        private async ValueTask ExecuteAsync(bool copy, CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
        {
            DiscordChannel sourceChannel = firstMessage.Channel!;
            if (lastMessage is not null)
            {
                if (lastMessage.Channel!.Id != sourceChannel.Id)
                {
                    await context.RespondAsync("Please ensure that both messages are from the same channel.");
                    return;
                }
                else if (lastMessage.Channel.Id < firstMessage.Id)
                {
                    await context.RespondAsync("Please ensure the first message is older than the last message.");
                    return;
                }
            }

            string action = copy ? "copying" : "forwarding";
            await context.RespondAsync($"Started {action} messages...");

            // Create a webhook builder to reuse for all messages
            DiscordWebhookBuilder webhookBuilder = new()
            {
                Username = $"{context.Guild!.CurrentMember.Username} (Message Mover)",
                AvatarUrl = context.Guild.CurrentMember.AvatarUrl,
                Content = $"I've started {action} messages. This may take a moment.\nFirst message: {firstMessage.JumpLink}\nLast message: {(lastMessage is null ? "Latest message." : lastMessage.JumpLink)}"
            };

            // Create the webhook
            DiscordWebhook webhook;
            if (channel.Type is DiscordChannelType.NewsThread or DiscordChannelType.PublicThread or DiscordChannelType.PrivateThread)
            {
                webhook = await channel.Parent.CreateWebhookAsync("Message Mover", Optional.FromNoValue<Stream>(), "Requested by " + context.User.Username);
                webhookBuilder.WithThreadId(channel.Id);
            }
            else
            {
                webhook = await channel.CreateWebhookAsync("Message Mover", Optional.FromNoValue<Stream>(), "Requested by " + context.User.Username);
            }

            List<DiscordUser> users = [];
            List<DiscordMessage> messages = [];
            try
            {
                await webhook.ExecuteAsync(webhookBuilder);
                await GatherMessagesAsync(messages, users, firstMessage, lastMessage);

                if (copy)
                {
                    await CopyMessagesAsync(await context.GetCultureAsync(), webhook, channel, messages);
                }
                else
                {
                    await ForwardMessagesAsync(webhook, channel, messages);
                }

                await webhook.ExecuteAsync(new DiscordWebhookBuilder()
                {
                    Username = $"{context.Guild.CurrentMember.Username} (Message Mover)",
                    AvatarUrl = context.Guild.CurrentMember.AvatarUrl,
                    Content = $"I've finished {action} messages. {messages.Count:N0} messages have been {(copy ? "copied" : "forwarded")}.\n-# Cc {string.Join(", ", users.OrderBy(user => user.Id).Select(x => x.Mention))}"
                });
            }
            finally
            {
                await webhook.DeleteAsync();
            }

            if (context.Channel.LastMessageId == (await context.GetResponseAsync())?.Id)
            {
                // If there haven't been any other messages between the last message and the response, edit the response.
                await context.EditResponseAsync($"{messages.Count:N0} messages have been {(copy ? "copied" : "forwarded")}.");
            }
            else
            {
                // If there have been other messages between the last message and the response, send a new message.
                await context.RespondAsync($"{messages.Count:N0} messages have been {(copy ? "copied" : "forwarded")}.");
            }
        }

        private static async ValueTask GatherMessagesAsync(List<DiscordMessage> messages, List<DiscordUser> users, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
        {
            // Gather all messages in the inclusive range.
            messages.Add(firstMessage);
            if (firstMessage.Author is not null && !users.Contains(firstMessage.Author))
            {
                users.Add(firstMessage.Author);
            }

            if (lastMessage?.Id == firstMessage.Id)
            {
                return;
            }

            bool foundLastMessage = lastMessage is null;
            await foreach (DiscordMessage message in firstMessage.Channel!.GetMessagesAfterAsync(firstMessage.Id))
            {
                if (message.Author is not null && !users.Contains(message.Author))
                {
                    users.Add(message.Author);
                }

                messages.Add(message);
                if (message.Id == lastMessage?.Id)
                {
                    foundLastMessage = true;
                    break;
                }
            }

            if (!foundLastMessage)
            {
                throw new ArgumentException("The last message was not found after the first message in the source channel.");
            }

            // Sort the messages by when they were sent
            messages.Sort((x, y) => x.Id.CompareTo(y.Id));
        }

        private async ValueTask ForwardMessagesAsync(DiscordWebhook webhook, DiscordChannel channel, List<DiscordMessage> messages)
        {
            DiscordWebhookBuilder webhookBuilder = new();
            if (channel.Type is DiscordChannelType.NewsThread or DiscordChannelType.PublicThread or DiscordChannelType.PrivateThread)
            {
                webhookBuilder.WithThreadId(channel.Id);
            }

            for (int i = 0; i < messages.Count; i++)
            {
                DiscordMessage message = messages[i];
                if (i == 0 || message.Author != messages[i - 1].Author)
                {
                    webhookBuilder.Username = message.Author!.GetDisplayName();
                    webhookBuilder.AvatarUrl = message.Author is DiscordMember member && !string.IsNullOrWhiteSpace(member.GuildAvatarUrl) ? member.GuildAvatarUrl : message.Author!.AvatarUrl;
                    webhookBuilder.Content = $"{message.Author.Mention} said:";
                    await webhook.ExecuteAsync(webhookBuilder);
                }

                // Forward the message
                await message.ForwardAsync(channel);
            }
        }

        private async ValueTask CopyMessagesAsync(CultureInfo cultureInfo, DiscordWebhook webhook, DiscordChannel channel, List<DiscordMessage> messages)
        {
            for (int messageIndex = 0; messageIndex < messages.Count; messageIndex++)
            {
                DiscordMessage message = messages[messageIndex];
                DiscordWebhookBuilder webhookBuilder = new(new DiscordMessageBuilder(message))
                {
                    Username = message.Author!.GetDisplayName(),
                    AvatarUrl = message.Author is DiscordMember member && !string.IsNullOrWhiteSpace(member.GuildAvatarUrl) ? member.GuildAvatarUrl : message.Author!.AvatarUrl
                };

                if (channel.Type is DiscordChannelType.NewsThread or DiscordChannelType.PublicThread or DiscordChannelType.PrivateThread)
                {
                    webhookBuilder.WithThreadId(channel.Id);
                }

                if (!string.IsNullOrWhiteSpace(message.ReferencedMessage?.Content) && message.ReferencedMessage.Content.Length + webhookBuilder.Content?.Length + 3 < 2000)
                {
                    webhookBuilder.WithContent($"> {message.ReferencedMessage.Content}\n{webhookBuilder.Content}");
                }

                if (message.Attachments.Count != 0)
                {
                    HashSet<string> attachments = new(StringComparer.Ordinal);
                    for (int attachmentIndex = 0; attachmentIndex < message.Attachments.Count; attachmentIndex++)
                    {
                        DiscordAttachment attachment = message.Attachments[attachmentIndex];
                        string fileName = attachment.FileName ?? $"attachment-{attachmentIndex}";
                        string uniqueFileName = fileName;
                        string baseName = Path.GetFileNameWithoutExtension(fileName);
                        string extension = Path.GetExtension(fileName);
                        int suffix = 1;
                        while (!attachments.Add(uniqueFileName))
                        {
                            uniqueFileName = $"{baseName} ({suffix.ToString(cultureInfo)}){extension}";
                            suffix++;
                        }

                        webhookBuilder.AddFile(uniqueFileName, await _httpClient.GetStreamAsync(attachment.Url), false);
                    }
                }

                //if (message.Components?.Count is not null and not 0)
                //{
                //    webhookBuilder.AddComponents(message.Components);
                //}

                //_webhookBuilderFlagsSetter(webhookBuilder, message.Flags ?? 0);
                await webhook.ExecuteAsync(webhookBuilder);
            }
        }
    }
}
