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

        public static bool operator ==(SingleReminderModel? left, SingleReminderModel? right) => Equals(left, right);
        public static bool operator !=(SingleReminderModel? left, SingleReminderModel? right) => !Equals(left, right);
        public override bool Equals(object? obj) => obj is SingleReminderModel model && Id.Equals(model.Id) && Type == model.Type && UserId == model.UserId && ChannelId == model.ChannelId && GuildId == model.GuildId && Message == model.Message && Time == model.Time && ProcrastinationCount == model.ProcrastinationCount;
        public override int GetHashCode() => HashCode.Combine(Id, Type, UserId, ChannelId, GuildId, Message, Time, ProcrastinationCount);
    }
}
