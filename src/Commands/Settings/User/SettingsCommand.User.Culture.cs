using System.Globalization;
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
            /// Sets the culture for the current user, which is responsible for formatting dates and numbers.
            /// </summary>
            [Command("culture"), DefaultGroupCommand]
            public static async ValueTask CultureAsync(CommandContext context, CultureInfo? culture = null)
            {
                UserSettingsModel? userSettings = await UserSettingsModel.GetUserSettingsAsync(context.User.Id);
                if (userSettings is null)
                {
                    await context.RespondAsync(SETTINGS_NOT_SETUP);
                    return;
                }
                else if (culture is null)
                {
                    await context.RespondAsync($"Your current culture is set to {userSettings.Culture.NativeName}/{userSettings.Culture.IetfLanguageTag}.");
                    return;
                }

                userSettings = userSettings with { Culture = culture };
                await UserSettingsModel.UpdateUserSettingsAsync(userSettings);
                await context.RespondAsync($"Your culture has been updated to {culture.NativeName}/{culture.IetfLanguageTag}.");
            }
        }
    }
}
