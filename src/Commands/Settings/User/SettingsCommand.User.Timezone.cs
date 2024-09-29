using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands
{
    public sealed partial class SettingsCommand
    {
        public static partial class UserSettingsCommand
        {
            /// <summary>
            /// Sets the timezone for the current user, which is responsible for time-based commands such as reminders.
            /// </summary>
            [Command("timezone"), DefaultGroupCommand]
            public static async ValueTask CultureAsync(CommandContext context, TimeZoneInfo? timezone = null)
            {
                UserSettingsModel? userSettings = await UserSettingsModel.GetUserSettingsAsync(context.User.Id);
                if (userSettings is null)
                {
                    await context.RespondAsync(SETTINGS_NOT_SETUP);
                    return;
                }
                else if (timezone is null)
                {
                    await context.RespondAsync($"Your current culture is set to {userSettings.Timezone.DisplayName}/{userSettings.Timezone.Id}.");
                    return;
                }

                userSettings = userSettings with { Timezone = timezone };
                await UserSettingsModel.UpdateUserSettingsAsync(userSettings);
                await context.RespondAsync($"Your culture has been updated to {timezone.DisplayName}/{timezone.Id}.");
            }
        }
    }
}
