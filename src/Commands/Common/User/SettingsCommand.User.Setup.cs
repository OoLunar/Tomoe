using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands
{
    public static partial class UserSettingsCommand
    {
        /// <summary>
        /// Sets the timezone for the current user, which is responsible for time-based commands such as reminders.
        /// </summary>
        [Command("setup"), DefaultGroupCommand]
        public static async ValueTask SetupAsync(CommandContext context, CultureInfo? culture = null, TimeZoneInfo? timezone = null)
        {
            UserSettingsModel userSettings = new()
            {
                UserId = context.User.Id,
                Culture = culture ?? CultureInfo.CurrentCulture,
                Timezone = timezone ?? TimeZoneInfo.Local
            };

            await UserSettingsModel.UpdateUserSettingsAsync(userSettings);
            await context.RespondAsync($"Your user settings have been set up with the culture {userSettings.Culture.NativeName}/{userSettings.Culture.IetfLanguageTag} and timezone {userSettings.Timezone.DisplayName}/{userSettings.Timezone.Id}.");
        }
    }
}
