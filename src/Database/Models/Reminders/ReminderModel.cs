using System;

namespace OoLunar.Tomoe.Database.Models.Reminders
{
    public interface IBaseReminderModel
    {
        Guid Id { get; init; }
        ReminderType Type { get; init; }
        ulong UserId { get; init; }
        ulong ChannelId { get; init; }
        ulong GuildId { get; init; }
        string Message { get; set; }
    }

    public enum ReminderType
    {
        Once,
        Repeating,
        Todo
    }
}
