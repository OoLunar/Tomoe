using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class MoveCommand : BaseCommand
    {
<<<<<<< HEAD
        private readonly HttpClient _httpClient;

        public MoveCommand(HttpClient httpClient) => _httpClient = httpClient;
=======
        public static readonly HttpClient HttpClient = new() { DefaultRequestHeaders = { { "User-Agent", "Tomoe Discord Bot/5.0" } } };
>>>>>>> b28dba9 (Uh oh)

        [Command("move"), Description("Moves a chunk of messages (inclusive) to a different channel.")]
        public async Task MoveAsync(CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
        {
            await context.DelayAsync();
            IEnumerable<DiscordMessage> messages = (await firstMessage.Channel.GetMessagesAfterAsync(firstMessage.Id)).Prepend(firstMessage);
            if (lastMessage != null)
            {
                messages = messages.OrderBy(x => x.CreationTimestamp).TakeWhile(m => m.Id != lastMessage.Id).Append(lastMessage);
            }

            DiscordWebhookBuilder webhookBuilder = new()
            {
                Username = $"{context.Guild!.CurrentMember.Username} (Message Mover)",
                AvatarUrl = context.Guild.CurrentMember.AvatarUrl,
                Content = $"Started moving messages. This may take awhile.\nFirst message: {firstMessage.JumpLink}\nLast message: {(lastMessage == null ? "Latest message." : lastMessage.JumpLink)}"
            };

            DiscordWebhook webhook;
            if (channel.Type is ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread)
            {
                webhook = await channel.Parent.CreateWebhookAsync("Message Mover", Optional.FromNoValue<Stream>(), "Requested by " + context.User.Username);
                webhookBuilder.WithThreadId(channel.Id);
            }
            else
            {
                webhook = await channel.CreateWebhookAsync("Message Mover", Optional.FromNoValue<Stream>(), "Requested by " + context.User.Username);
            }

            await webhook.ExecuteAsync(webhookBuilder);
            foreach (DiscordMessage message in messages.OrderBy(x => x.CreationTimestamp))
            {
<<<<<<< HEAD
                webhookBuilder = new DiscordWebhookBuilder(new DiscordMessageBuilder(message));
=======
                webhookBuilder = (DiscordWebhookBuilder)(IDiscordMessageBuilder)new DiscordMessageBuilder(message);
>>>>>>> b28dba9 (Uh oh)
                DiscordMember? member = (DiscordMember)message.Author;
                webhookBuilder.WithUsername(member.DisplayName);
                webhookBuilder.WithAvatarUrl(member.GuildAvatarUrl ?? member.AvatarUrl);
                if (channel.Type is ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread)
                {
                    webhookBuilder.WithThreadId(channel.Id);
                }

                if (message.ReferencedMessage is not null && message.ReferencedMessage.Content.Length + webhookBuilder.Content.Length + 3 < 2000)
                {
                    webhookBuilder.WithContent($"> {message.ReferencedMessage.Content}\n{webhookBuilder.Content}");
                }

                if (message.Attachments.Count != 0)
                {
<<<<<<< HEAD
                    List<string> attachments = new();
                    for (int i = 0; i < message.Attachments.Count; i++)
                    {
                        DiscordAttachment attachment = message.Attachments[i];
                        if (attachments.Contains(attachment.FileName))
                        {
                            webhookBuilder.AddFile(attachment.FileName + i.ToString(CultureInfo.InvariantCulture), await _httpClient.GetStreamAsync(attachment.Url), false);
                        }
                        else
                        {
                            webhookBuilder.AddFile(attachment.FileName, await _httpClient.GetStreamAsync(attachment.Url), false);
                        }
                        attachments.Add(attachment.FileName);
                    }
=======
                    Dictionary<string, Stream> attachments = new();
                    for (int i = 0; i < message.Attachments.Count; i++)
                    {
                        DiscordAttachment attachment = message.Attachments[i];
                        if (attachments.ContainsKey(attachment.FileName))
                        {
                            webhookBuilder.AddFile(attachment.FileName + i.ToString(CultureInfo.InvariantCulture), await HttpClient.GetStreamAsync(attachment.Url), true);
                        }
                        else
                        {
                            webhookBuilder.AddFile(attachment.FileName, await HttpClient.GetStreamAsync(attachment.Url), true);
                        }
                    }
                    webhookBuilder.AddFiles(attachments, true);
>>>>>>> b28dba9 (Uh oh)
                }

                if (message.Components.Count != 0)
                {
                    webhookBuilder.AddComponents(message.Components);
                }

                await webhook.ExecuteAsync(webhookBuilder);
            }
            await webhook.ExecuteAsync(new DiscordWebhookBuilder().WithUsername(context.Guild.CurrentMember.Username + " (Message Mover)").WithAvatarUrl(context.Guild.CurrentMember.AvatarUrl).WithContent($"Messages have been moved."));
            await webhook.DeleteAsync();
            await context.ReplyAsync($"{messages.Count()} messages have been moved.");
        }
    }
}
