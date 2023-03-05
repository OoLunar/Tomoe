using System;

namespace OoLunar.Tomoe.Database.Models.Reminders
{
    public sealed class RepeatingReminderModel : IBaseReminderModel
    {
        public Guid Id { get; init; }
        public ReminderType Type { get; init; }
        public ulong UserId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong GuildId { get; init; }
        public string Message { get; set; }
        public DayOfWeek[] DaysOfWeek { get; init; }
        public TimeSpan[] ExpireTimes { get; init; }
    }
}
