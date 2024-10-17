using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Moved my heart.
    /// </summary>
    public sealed class MoveCommand
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Creates a new instance of <see cref="MoveCommand"/>.
        /// </summary>
        /// <param name="httpClient">Required service for retrieving remote files.</param>
        public MoveCommand(HttpClient httpClient) => this.httpClient = httpClient;

        /// <summary>
        /// Copies a chunk of messages to a different channel.
        /// </summary>
        /// <param name="channel">Which channel to send the messages to.</param>
        /// <param name="firstMessage">Where to start copying messages from.</param>
        /// <param name="lastMessage">Where to stop copying messages from. If not provided, will copy all messages after the first message.</param>
        [Command("move"), Description("Moves a chunk of messages (inclusive) to a different channel."), RequirePermissions(DiscordPermissions.ManageMessages | DiscordPermissions.ReadMessageHistory)]
        public async ValueTask MoveAsync(CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
        {
            await context.DeferResponseAsync();

            List<DiscordUser> users = [];
            List<DiscordMessage> messages = [];
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

            DiscordWebhookBuilder webhookBuilder = new()
            {
                Username = $"{context.Guild!.CurrentMember.Username} (Message Mover)",
                AvatarUrl = context.Guild.CurrentMember.AvatarUrl,
                Content = $"I've started moving messages. This may take a moment.\nFirst message: {firstMessage.JumpLink}\nLast message: {(lastMessage is null ? "Latest message." : lastMessage.JumpLink)}"
            };

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
            foreach (DiscordMessage message in messages.OrderBy(x => x.Id))
            {
                webhookBuilder = new DiscordWebhookBuilder(new DiscordMessageBuilder(message));
                webhookBuilder.WithUsername(message.Author!.GetDisplayName());
                webhookBuilder.WithAvatarUrl(message.Author is DiscordMember member ? member.GuildAvatarUrl : message.Author!.AvatarUrl);
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
                    for (int i = 0; i < message.Attachments.Count; i++)
                    {
                        DiscordAttachment attachment = message.Attachments[i];
                        if (attachments.Contains(attachment.FileName!))
                        {
                            webhookBuilder.AddFile(attachment.FileName + i.ToString(await context.GetCultureAsync()), await httpClient.GetStreamAsync(attachment.Url), false);
                        }
                        else
                        {
                            webhookBuilder.AddFile(attachment.FileName!, await httpClient.GetStreamAsync(attachment.Url), false);
                        }

                        attachments.Add(attachment.FileName!);
                    }
                }

                if (message.Components?.Count is not null and not 0)
                {
                    webhookBuilder.AddComponents(message.Components);
                }

                await webhook.ExecuteAsync(webhookBuilder);
            }

            await webhook.ExecuteAsync(new DiscordWebhookBuilder().WithUsername(context.Guild.CurrentMember.Username + " (Message Mover)").WithAvatarUrl(context.Guild.CurrentMember.AvatarUrl).WithContent($"I've finished moving messages. {messages.Count:N0} messages have been moved.\n-# Cc {string.Join(", ", users.OrderBy(user => user.Id).Select(x => x.Mention))}"));
            await webhook.DeleteAsync();
            await context.RespondAsync($"{messages.Count:N0} messages have been moved.");
        }
    }
}
