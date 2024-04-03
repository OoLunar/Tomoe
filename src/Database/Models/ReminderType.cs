namespace OoLunar.Tomoe.Database.Models
{
    public enum ReminderType : byte
    {
        /// <summary>
        /// The reminder is a one-time reminder.
        /// </summary>
        OneTime,

        /// <summary>
        /// The reminder is a recurring reminder.
        /// </summary>
        Recurring
    }
}
