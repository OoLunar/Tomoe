using System;
using System.ComponentModel.DataAnnotations;
using Tomoe.Commands.Moderation;

namespace Tomoe.Db
{
    public class LogSetting
    {
        [Key]
        public int Id { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; internal set; }
        public CustomEvent? CustomEvent { get; init; }
        public DiscordEvent? DiscordEvent { get; init; }
        public string Format { get; internal set; }
        public bool IsLoggingEnabled { get; internal set; }

        public LogSetting() { }

        public LogSetting(ulong guildId, ulong channelId, CustomEvent? customEvent, DiscordEvent? discordEvent, string format, bool isLoggingEnabled)
        {
            GuildId = guildId;
            ChannelId = channelId;
            CustomEvent = customEvent;
            DiscordEvent = discordEvent;
            Format = string.IsNullOrWhiteSpace(format) ? throw new ArgumentException("Format cannot be null or whitespace.", nameof(format)) : format;
            IsLoggingEnabled = isLoggingEnabled;
        }
    }
}
