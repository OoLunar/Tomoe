using System;
using System.ComponentModel.DataAnnotations;
using Tomoe.Commands.Moderation;

namespace Tomoe.Db
{
    public class Reminder
    {
        [Key]
        public int Id { get; init; }
        public int LogId { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong MessageId { get; init; }
        public ulong UserId { get; init; }
        public string JumpLink { get; init; }
        public string Content { get; internal set; }
        public bool Expires { get; internal set; }
        public DateTime ExpiresOn { get; internal set; }
        public DateTime IssuedAt { get; init; } = DateTime.UtcNow;
        public CustomEvent Punishment { get; init; }

        public Reminder(int logId, ulong guildId, ulong channelId, ulong messageId, ulong userId, string jumpLink, string content, bool expires, DateTime expiresOn, Commands.Moderation.CustomEvent punishment)
        {
            LogId = logId;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            UserId = userId;
            JumpLink = string.IsNullOrWhiteSpace(jumpLink) ? throw new ArgumentException("Jump link cannot be null or whitespace.", nameof(jumpLink)) : jumpLink;
            Content = content;
            Expires = expires;
            ExpiresOn = expiresOn;
            Punishment = punishment;
        }
    }
}
