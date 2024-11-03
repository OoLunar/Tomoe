using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Flags")]
        private static extern void _webhookBuilderFlagsSetter(BaseDiscordMessageBuilder<DiscordWebhookBuilder> message, DiscordMessageFlags flags);

        /// <summary>
        /// Cached <see cref="TimeSpan"/> of 10 seconds.
        /// </summary>
        private static readonly TimeSpan _tenSeconds = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Used to fetch attachments from Discord.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// We're using a ULID's timestamp to store when the last message was sent (to detect ratelimits)
        /// and the randomness to store the completion percentage.
        /// </summary>
        private Ulid _completionPercent;

        /// <summary>
        /// Creates a new instance of <see cref="CopyCommand"/>.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for fetching attachments.</param>
        public CopyCommand(HttpClient httpClient) => _httpClient = httpClient;

        /// <summary>
        /// Copies a chunk of messages to a different channel.
        /// </summary>
        /// <param name="channel">Which channel to send the messages to.</param>
        /// <param name="firstMessage">Where to start copying messages from. This message will be moved too.</param>
        /// <param name="lastMessage">Where to stop copying messages from. If not provided, will copy all messages after the first message.</param>
        [Command("copy"), TextAlias("move"), Description("Copies a chunk of messages (inclusive) to a different channel."), RequirePermissions(DiscordPermissions.ManageMessages | DiscordPermissions.ReadMessageHistory)]
        public async ValueTask CopyAsync(CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
            => await ExecuteAsync(true, context, channel, firstMessage, lastMessage);

        /// <summary>
        /// Forwards a chunk of messages to a different channel.
        /// </summary>
        /// <param name="channel">Which channel to send the messages to.</param>
        /// <param name="firstMessage">Where to start copying messages from. This message will be moved too.</param>
        /// <param name="lastMessage">Where to stop copying messages from. If not provided, will copy all messages after the first message.</param>
        [Command("forward"), Description("Forwards a chunk of messages (inclusive) to a different channel."), RequirePermissions(DiscordPermissions.ManageMessages | DiscordPermissions.ReadMessageHistory)]
        public async ValueTask ForwardAsync(CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
            => await ExecuteAsync(false, context, channel, firstMessage, lastMessage);

        private async ValueTask ExecuteAsync(bool copy, CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
        {
            await context.RespondAsync("Moving messages...");

            CancellationTokenSource cancellationTokenSource = new();
            Task idleTask = IdleAsync(context, channel, cancellationTokenSource.Token);

            // Create a webhook builder to reuse for all messages
            DiscordWebhookBuilder webhookBuilder = new()
            {
                Username = $"{context.Guild!.CurrentMember.Username} (Message Mover)",
                AvatarUrl = context.Guild.CurrentMember.AvatarUrl,
                Content = $"I've started moving messages. This may take a moment.\nFirst message: {firstMessage.JumpLink}\nLast message: {(lastMessage is null ? "Latest message." : lastMessage.JumpLink)}"
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

            await webhook.ExecuteAsync(webhookBuilder);

            List<DiscordUser> users = [];
            List<DiscordMessage> messages = [];
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
                Content = $"I've finished moving messages. {messages.Count:N0} messages have been moved.\n-# Cc {string.Join(", ", users.OrderBy(user => user.Id).Select(x => x.Mention))}"
            });

            await webhook.DeleteAsync();
            await cancellationTokenSource.CancelAsync();
            await idleTask;

            if (context.Channel.LastMessageId == (await context.GetResponseAsync())?.Id)
            {
                // If there haven't been any other messages between the last message and the response, edit the response.
                await context.EditResponseAsync($"{messages.Count:N0} messages have been moved.");
            }
            else
            {
                // If there have been other messages between the last message and the response, send a new message.
                await context.RespondAsync($"{messages.Count:N0} messages have been moved.");
            }
        }

        private async Task IdleAsync(CommandContext context, DiscordChannel channel, CancellationToken cancellationToken = default)
        {
            // Lock the channel to prevent other messages from being sent
            List<DiscordOverwriteBuilder> overwrites = [.. channel.PermissionOverwrites.Select(DiscordOverwriteBuilder.From)];
            List<DiscordOverwriteBuilder> newOverwrites = [];
            foreach (DiscordOverwriteBuilder overwrite in overwrites)
            {
                // Shallow copy
                DiscordOverwriteBuilder newOverwrite = overwrite with { };

                // If we're moving messages into threads, prevent users from sending messages in threads
                newOverwrite.Deny(channel.IsThread
                    ? DiscordPermissions.SendMessagesInThreads | DiscordPermissions.ManageMessages
                    : DiscordPermissions.SendMessages | DiscordPermissions.ManageMessages | DiscordPermissions.CreatePublicThreads | DiscordPermissions.CreatePrivateThreads);

                newOverwrites.Add(newOverwrite);
            }

            // Add an overwrite that'll allow the bot to send messages
            newOverwrites.Add(new DiscordOverwriteBuilder(channel.Guild.CurrentMember)
            {
                Allowed = channel.IsThread ? DiscordPermissions.SendMessagesInThreads : DiscordPermissions.SendMessages
            });

            // Remove send messages, manage messages (pins), and create
            // threads permissions so the message history doesn't get fucked.
            await channel.ModifyAsync(channelEditModel => channelEditModel.PermissionOverwrites = newOverwrites);

            // Periodically send a typing indicator to let others know the bot is still working.
            PeriodicTimer timer = new(_tenSeconds);
            cancellationToken.Register(timer.Dispose);

            Ulid lastCompletionPercent = _completionPercent;
            while (await timer.WaitForNextTickAsync(default))
            {
                await channel.TriggerTypingAsync();
                if (_completionPercent != lastCompletionPercent)
                {
                    StringBuilder stringBuilder = new($"Moving messages... {_completionPercent.Random[0]}% done...");

                    // If it's been more than 10 seconds since the time has last updated, we've hit a ratelimit
                    if ((DateTimeOffset.UtcNow - _completionPercent.Time) > _tenSeconds)
                    {
                        stringBuilder.Append(" Warning! Discord's ratelimits have been hit! Per Discord's request, the bot will pause for a few minutes before continuing.");
                    }

                    await context.EditResponseAsync(stringBuilder.ToString());
                    lastCompletionPercent = _completionPercent;
                }
            }

            // Restore the previous permission overwrites
            await channel.ModifyAsync(channelEditModel => channelEditModel.PermissionOverwrites = overwrites);
        }

        private static async ValueTask GatherMessagesAsync(List<DiscordMessage> messages, List<DiscordUser> users, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
        {
            // Gather all messages to move
            await foreach (DiscordMessage message in firstMessage.Channel!.GetMessagesAfterAsync(firstMessage.Id))
            {
                if (!users.Contains(message.Author!))
                {
                    users.Add(message.Author!);
                }

                messages.Add(message);
                if (message.Id == lastMessage?.Id)
                {
                    break;
                }
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
                _completionPercent = Ulid.NewUlid(DateTimeOffset.UtcNow, [(byte)Math.Clamp((int)(i / (double)messages.Count * 100), 0, 100), 0, 0, 0, 0, 0, 0, 0, 0, 0]);
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
                    List<string> attachments = [];
                    for (int attachmentIndex = 0; attachmentIndex < message.Attachments.Count; attachmentIndex++)
                    {
                        DiscordAttachment attachment = message.Attachments[attachmentIndex];
                        if (attachments.Contains(attachment.FileName!))
                        {
                            webhookBuilder.AddFile(attachment.FileName + attachmentIndex.ToString(cultureInfo), await _httpClient.GetStreamAsync(attachment.Url), false);
                        }
                        else
                        {
                            webhookBuilder.AddFile(attachment.FileName!, await _httpClient.GetStreamAsync(attachment.Url), false);
                        }

                        attachments.Add(attachment.FileName!);
                    }
                }

                if (message.Components?.Count is not null and not 0)
                {
                    webhookBuilder.AddComponents(message.Components);
                }

                _webhookBuilderFlagsSetter(webhookBuilder, message.Flags ?? 0);
                await webhook.ExecuteAsync(webhookBuilder);
                _completionPercent = Ulid.NewUlid(DateTimeOffset.UtcNow, [(byte)Math.Clamp((int)(messageIndex / (double)messages.Count * 100), 0, 100), 0, 0, 0, 0, 0, 0, 0, 0, 0]);
            }
        }
    }
}
