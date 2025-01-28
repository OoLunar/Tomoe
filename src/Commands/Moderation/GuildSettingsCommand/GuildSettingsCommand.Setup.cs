using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Interactivity.Moments.Confirm;
using OoLunar.Tomoe.Interactivity.Moments.Prompt;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed partial class GuildSettingsCommand
    {
        /// <summary>
        /// Configures the guild settings.
        /// </summary>
        [Command("setup")]
        [RequirePermissions([DiscordPermission.ManageNicknames], [])]
        public static async ValueTask SetupAsync(CommandContext context)
        {
            bool? enableAutoDehoist = await context.ConfirmAsync("Would you like to enable auto-dehoist?");
            if (enableAutoDehoist is null)
            {
                await context.RespondAsync("Timed out! The guild settings have not been updated.");
                return;
            }

            string? dehoistFormat = await context.PromptAsync("Please enter the dehoist format: ", "Two placeholders are available: `{display_name}` and `{user_id}`.");
            if (dehoistFormat is null)
            {
                await context.RespondAsync("Timed out! The guild settings have not been updated.");
                return;
            }

            bool? enableRestoreRoles = await context.ConfirmAsync("Would you like to restore roles when a member rejoins the server?");
            if (enableRestoreRoles is null)
            {
                await context.RespondAsync("Timed out! The guild settings have not been updated.");
                return;
            }

            string? textPrefix = await context.PromptAsync("Please enter the new text command prefix: ", "Type `default` to reset to the default prefix.");
            if (textPrefix is null)
            {
                await context.RespondAsync("Timed out! The guild settings have not been updated.");
                return;
            }
            else if (textPrefix is "default" or "`default`")
            {
                textPrefix = null;
            }

            GuildSettingsModel settings = new()
            {
                GuildId = context.Guild!.Id,
                AutoDehoist = enableAutoDehoist.Value,
                AutoDehoistFormat = dehoistFormat,
                RestoreRoles = enableRestoreRoles.Value,
                TextPrefix = textPrefix
            };

            await GuildSettingsModel.UpdateSettingsAsync(settings);
            await context.RespondAsync("Guild settings have been updated.");
        }
    }
}
