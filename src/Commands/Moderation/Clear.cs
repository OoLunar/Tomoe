using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.Extensions.Logging;
using Tomoe.Enums;

namespace Tomoe.Commands.Moderation
{
    public class Clear : BaseCommandModule
    {
        public ILogger<Clear> Logger { private get; init; } = null!;

        [Command("clear"), Description("Clears messages from chat."), RequireGuild]
        public async Task ClearChannelAsync(CommandContext context, [Description("Which channel to clear the messages from.")] DiscordChannel? channel = null, [Description("How many messages to clear.")] int messageCount = 5, [Description("Which message to stop clearing at.")] DiscordMessage? beforeMessage = null, [Description("Which message to start clearing at.")] DiscordMessage? afterMessage = null, [Description("What type of messages to clear.")] FilterType filterType = FilterType.AllMessages, [Description("An optional argument that may be required when using certain kinds of filter types.")] string? filterTypeArgument = null)
        {
            channel ??= context.Channel;
            if (!channel.PermissionsFor(context.Member).HasPermission(Permissions.ManageMessages))
            {
                await context.RespondAsync($"[Error]: You cannot clear messages in {channel.Mention} due to Discord permissions!");
                return;
            }
            else if (!channel.PermissionsFor(context.Guild.CurrentMember).HasPermission(Permissions.ManageMessages))
            {
                await context.RespondAsync($"[Error]: I cannot clear messages in {channel.Mention} due to Discord permissions!");
                return;
            }

            if (messageCount is <= 2 or > 100)
            {
                await context.RespondAsync("Invalid message count. Please select a range between 2-100.");
                return;
            }

            IEnumerable<DiscordMessage> messages;
#pragma warning disable IDE0045 // Convert to conditional expression
            if (beforeMessage != null && afterMessage != null)
            {
                await context.RespondAsync("You cannot specify both before and after messages.");
                return;
            }
            else if (beforeMessage != null)
            {
                messages = await channel.GetMessagesBeforeAsync(beforeMessage.Id, messageCount);
            }
            else if (afterMessage != null)
            {
                messages = await channel.GetMessagesAfterAsync(afterMessage.Id, messageCount);
            }
            else
            {
                messages = await channel.GetMessagesAsync(messageCount);
            }
#pragma warning restore IDE0045 // Convert to conditional expression

            if (!messages.Any())
            {
                await context.RespondAsync("No messages to clear.");
                return;
            }
            messages = messages.Where(message => message.CreationTimestamp >= DateTime.UtcNow.AddDays(-14));

            messages = filterType switch
            {
                FilterType.AllMessages => messages,
                FilterType.Phrase => FilterPhrase(messages, filterTypeArgument),
                FilterType.Command => FilterCommand(messages),
                FilterType.Embed => FilterEmbed(messages),
                FilterType.File => FilterFile(messages),
                FilterType.Attachment => FilterAttachment(messages),
                FilterType.UserPing => FilterUserPing(messages),
                FilterType.RolePing => FilterRolePing(messages),
                FilterType.Ping => FilterPing(messages),
                FilterType.Regex => FilterRegex(messages, filterTypeArgument),
                _ => throw new ArgumentOutOfRangeException(nameof(filterType), filterType, null)
            };

            try
            {
                await channel.DeleteMessagesAsync(messages);
            }
            catch (DiscordException error)
            {
                await context.RespondAsync($"[Error]: Failed to clear {messages.Count().ToMetric()} messages. Error: (HTTP {error.WebResponse.ResponseCode}) {error.JsonMessage}");
                Logger.LogWarning(error, "Failed to clear {MessageCount} from guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", messages.Count().ToMetric(), context.Guild.Id, error.WebResponse.ResponseCode, error.JsonMessage);
                return;
            }
            await context.RespondAsync($"Cleared {messages.Count().ToMetric()} messages.");
        }

        [Command("clear"), Description("Clears messages from chat.")]
        public async Task ClearUsersAsync(CommandContext context, [Description("How many messages to clear.")] int messageCount = 5, [Description("What type of messages to clear.")] FilterType filterType = FilterType.AllMessages, [Description("An optional argument that may be required when using certain kinds of filter types.")] string? filterTypeArgument = null, [Description("Which messages to delete that belong to these users.")] params ulong[] userIds)
        {
            if (messageCount is <= 2 or > 100)
            {
                await context.RespondAsync("Invalid message count. Please select a range between 2-100.");
                return;
            }
            else if (!userIds.Any()) // Check may be redundant
            {
                await context.RespondAsync("No users to clear.");
                return;
            }
            int totalMessageCount = 0;
            Dictionary<DiscordChannel, bool> failedChannels = new(); // true if they failed, false I failed

            foreach (DiscordChannel channel in context.Guild.Channels.Values)
            {
                if (!channel.PermissionsFor(context.Member).HasPermission(Permissions.ManageMessages))
                {
                    failedChannels.Add(channel, true);
                    continue;
                }
                else if (!channel.PermissionsFor(context.Guild.CurrentMember).HasPermission(Permissions.ManageMessages))
                {
                    failedChannels.Add(channel, false);
                    continue;
                }

                IEnumerable<DiscordMessage> messages = await channel.GetMessagesAsync(messageCount);
                if (!messages.Any())
                {
                    continue;
                }
                messages = messages.Where(message => message.CreationTimestamp >= DateTime.UtcNow.AddDays(-14) && userIds.Contains(message.Author.Id));

                messages = filterType switch
                {
                    FilterType.AllMessages => messages,
                    FilterType.Phrase => FilterPhrase(messages, filterTypeArgument),
                    FilterType.Command => FilterCommand(messages),
                    FilterType.Embed => FilterEmbed(messages),
                    FilterType.File => FilterFile(messages),
                    FilterType.Attachment => FilterAttachment(messages),
                    FilterType.UserPing => FilterUserPing(messages),
                    FilterType.RolePing => FilterRolePing(messages),
                    FilterType.Ping => FilterPing(messages),
                    FilterType.Regex => FilterRegex(messages, filterTypeArgument),
                    _ => throw new ArgumentOutOfRangeException(nameof(filterType), filterType, null)
                };
                try
                {
                    await channel.DeleteMessagesAsync(messages);
                }
                catch (DiscordException error)
                {
                    failedChannels.Add(channel, false);
                    Logger.LogWarning(error, "Failed to clear {MessageCount} in channel {ChannelId} from guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", messages.Count().ToMetric(), context.Channel.Id, context.Guild.Id, error.WebResponse.ResponseCode, error.JsonMessage);
                    return;
                }
                totalMessageCount += messages.Count();
            }

            StringBuilder sb = new();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Cleared {totalMessageCount.ToMetric()} messages.");
            if (failedChannels.Count != 0)
            {
                sb.AppendLine("[Error]: Failed to clear messages in the following channels due to Discord permissions:");
                foreach (KeyValuePair<DiscordChannel, bool> kvp in failedChannels)
                {
                    sb.AppendLine(kvp.Value ? $"{kvp.Key.Mention} (I)" : $"{kvp.Key.Mention} (You)");
                }
            }
            await context.RespondAsync(sb.ToString());
        }

        public IEnumerable<DiscordMessage> FilterPhrase(IEnumerable<DiscordMessage> messages, string? phrase = null) => string.IsNullOrWhiteSpace(phrase) ? messages : messages.Where(message => message.Content.Contains(phrase, StringComparison.InvariantCultureIgnoreCase));
        public IEnumerable<DiscordMessage> FilterCommand(IEnumerable<DiscordMessage> messages) => messages.Where(message => message.Author.IsBot);
        public IEnumerable<DiscordMessage> FilterEmbed(IEnumerable<DiscordMessage> messages) => messages.Where(message => message.Embeds.Count != 0);
        public IEnumerable<DiscordMessage> FilterFile(IEnumerable<DiscordMessage> messages) => messages.Where(message => message.Attachments.Count != 0);
        public IEnumerable<DiscordMessage> FilterAttachment(IEnumerable<DiscordMessage> messages) => messages.Where(message => message.Attachments.Count != 0 || message.Embeds.Count != 0);
        public IEnumerable<DiscordMessage> FilterUserPing(IEnumerable<DiscordMessage> messages) => messages.Where(message => message.MentionedUsers.Count != 0);
        public IEnumerable<DiscordMessage> FilterRolePing(IEnumerable<DiscordMessage> messages) => messages.Where(message => message.MentionedRoles.Count != 0 || message.MentionEveryone);
        public IEnumerable<DiscordMessage> FilterPing(IEnumerable<DiscordMessage> messages) => messages.Where(message => message.MentionedUsers.Count != 0 || message.MentionedRoles.Count != 0);
        public IEnumerable<DiscordMessage> FilterRegex(IEnumerable<DiscordMessage> messages, string? regex = null) => string.IsNullOrWhiteSpace(regex) ? messages : messages.Where(message => Regex.Matches(message.Content, regex).Count != 0);
    }
}
