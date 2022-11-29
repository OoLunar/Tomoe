using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Moderation
{
	public class Move : BaseCommandModule
	{
		public static readonly HttpClient HttpClient = new() { DefaultRequestHeaders = { { "User-Agent", "Tomoe Discord Bot/3.0" } } };

		[Command("move"), Description("Moves a chunk of messages (inclusive) to a different channel."), RequireOwner]
		public async Task MoveAsync(CommandContext context, DiscordChannel channel, DiscordMessage firstMessage, DiscordMessage? lastMessage = null)
		{
			IEnumerable<DiscordMessage> messages = (await firstMessage.Channel.GetMessagesAfterAsync(firstMessage.Id)).Prepend(firstMessage);
			if (lastMessage != null)
			{
				messages = messages.OrderBy(x => x.CreationTimestamp).TakeWhile(m => m.Id != lastMessage.Id).Append(lastMessage);
			}

			DiscordWebhookBuilder webhookBuilder = new()
			{
				Username = $"{context.Guild.CurrentMember.Username} (Message Mover)",
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
				webhookBuilder = new();
				DiscordMember? member = (DiscordMember)message.Author;
				webhookBuilder.WithUsername(member.DisplayName);
				webhookBuilder.WithAvatarUrl(member.GuildAvatarUrl ?? member.AvatarUrl);
				if (channel.Type is ChannelType.NewsThread or ChannelType.PublicThread or ChannelType.PrivateThread)
				{
					webhookBuilder.WithThreadId(channel.Id);
				}

				if (!string.IsNullOrWhiteSpace(message.Content))
				{
					webhookBuilder.WithContent(message.Content);
				}

				if (message.ReferencedMessage is not null && message.ReferencedMessage.Content.Length + webhookBuilder.Content.Length + 3 < 2000)
				{
					webhookBuilder.WithContent($"> {message.ReferencedMessage.Content}\n{webhookBuilder.Content}");
				}

				if (message.Embeds.Count != 0)
				{
					webhookBuilder.AddEmbeds(message.Embeds);
				}

				if (message.Attachments.Count != 0)
				{
					Dictionary<string, Stream> attachments = new();
					foreach (DiscordAttachment attachment in message.Attachments)
					{
						if (attachments.ContainsKey(attachment.FileName))
						{
							attachments.Add(attachment.FileName + "1", await HttpClient.GetStreamAsync(attachment.Url));
						}
						else
						{
							attachments.Add(attachment.FileName, await HttpClient.GetStreamAsync(attachment.Url));
						}
					}
					webhookBuilder.AddFiles(attachments);
				}

				if (message.Components.Count != 0)
				{
					webhookBuilder.AddComponents(message.Components);
				}

				await webhook.ExecuteAsync(webhookBuilder);
			}
			await webhook.ExecuteAsync(new DiscordWebhookBuilder().WithUsername(context.Guild.CurrentMember.Username + " (Message Mover)").WithAvatarUrl(context.Guild.CurrentMember.AvatarUrl).WithContent($"Messages have been moved."));
			await webhook.DeleteAsync();
		}
	}
}
