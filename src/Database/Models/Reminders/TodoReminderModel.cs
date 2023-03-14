using System;

namespace OoLunar.Tomoe.Database.Models.Reminders
{
    public sealed class TodoReminderModel : IBaseReminderModel
    {
        public Guid Id { get; init; }
        public ReminderType Type { get; init; }
        public ulong UserId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong GuildId { get; init; }
        public string Message { get; set; }
        public TodoStatus Status { get; set; }

        public static bool operator ==(TodoReminderModel? left, TodoReminderModel? right) => Equals(left, right);
        public static bool operator !=(TodoReminderModel? left, TodoReminderModel? right) => !Equals(left, right);
        public override bool Equals(object? obj) => obj is TodoReminderModel model && Id.Equals(model.Id) && Type == model.Type && UserId == model.UserId && ChannelId == model.ChannelId && GuildId == model.GuildId && Message == model.Message && Status == model.Status;
        public override int GetHashCode() => HashCode.Combine(Id, Type, UserId, ChannelId, GuildId, Message, Status);
    }

    public enum TodoStatus
    {
        Pending,
        Completed,
        Cancelled
    }
}
