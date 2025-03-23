using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class UserSettingsCommand
    {
        private const string SETTINGS_NOT_SETUP = "Your user settings have not been set up yet! Please run `/settings user setup` to set them up.";

        /// <summary>
        /// Lists all the settings for the current user.
        /// </summary>
        [Command("list"), DefaultGroupCommand]
        public static async ValueTask ListSettingsAsync(CommandContext context)
        {
            UserSettingsModel? userSettings = await UserSettingsModel.GetUserSettingsAsync(context.User.Id);
            if (userSettings is null)
            {
                await context.RespondAsync(SETTINGS_NOT_SETUP);
                return;
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "User Settings",
                Color = new DiscordColor(0x6b73db)
            };

            embedBuilder.AddField("Culture", $"{userSettings.Culture.NativeName}/{userSettings.Culture.IetfLanguageTag}", true);
            embedBuilder.AddField("Timezone", userSettings.Timezone.Id, true);
            await context.RespondAsync(embedBuilder);
        }
    }
}
