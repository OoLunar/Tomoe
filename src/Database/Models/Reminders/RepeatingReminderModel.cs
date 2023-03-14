using System;
using System.Collections.Generic;

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

        public static bool operator ==(RepeatingReminderModel? left, RepeatingReminderModel? right) => Equals(left, right);
        public static bool operator !=(RepeatingReminderModel? left, RepeatingReminderModel? right) => !Equals(left, right);
        public override bool Equals(object? obj) => obj is RepeatingReminderModel model && Id.Equals(model.Id) && Type == model.Type && UserId == model.UserId && ChannelId == model.ChannelId && GuildId == model.GuildId && Message == model.Message && EqualityComparer<DayOfWeek[]>.Default.Equals(DaysOfWeek, model.DaysOfWeek) && EqualityComparer<TimeSpan[]>.Default.Equals(ExpireTimes, model.ExpireTimes);
        public override int GetHashCode() => HashCode.Combine(Id, Type, UserId, ChannelId, GuildId, Message, DaysOfWeek, ExpireTimes);
    }
}
