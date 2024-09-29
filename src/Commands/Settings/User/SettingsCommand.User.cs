using DSharpPlus.Commands;

namespace OoLunar.Tomoe.Commands
{
    public sealed partial class SettingsCommand
    {
        /// <summary>
        /// Manages settings for the current user, such as their culture and timezone.
        /// </summary>
        [Command("user")]
        public static partial class UserSettingsCommand;
    }
}
