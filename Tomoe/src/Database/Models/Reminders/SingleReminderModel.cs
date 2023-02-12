using System;

namespace OoLunar.Tomoe.Database.Models.Reminders
{
    public sealed class SingleReminderModel : IBaseReminderModel
    {
        public Guid Id { get; init; }
        public ReminderType Type { get; init; }
        public ulong UserId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong GuildId { get; init; }
        public string Message { get; set; }
        public DateTime Time { get; init; }
        public byte ProcrastinationCount { get; set; }
    }
}
