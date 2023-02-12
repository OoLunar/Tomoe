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
    }

    public enum TodoStatus
    {
        Pending,
        Completed,
        Cancelled
    }
}
