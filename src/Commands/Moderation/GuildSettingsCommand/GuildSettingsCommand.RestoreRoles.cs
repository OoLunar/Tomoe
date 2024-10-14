using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed partial class GuildSettingsCommand
    {
        /// <summary>
        /// Whether or not to restore roles when a member rejoins the server.
        /// </summary>
        /// <param name="restoreRoles">Whether or not to restore roles when a member rejoins the server.</param>
        [Command("restore_roles")]
        [RequirePermissions(DiscordPermissions.ManageRoles, DiscordPermissions.ManageGuild)]
        public static async ValueTask RestoreRolesAsync(CommandContext context, bool restoreRoles = false)
        {
            GuildSettingsModel? settings = await GuildSettingsModel.GetSettingsAsync(context.Guild!.Id);
            if (settings is null)
            {
                await context.RespondAsync(NOT_SETUP_TEXT);
                return;
            }

            await GuildSettingsModel.UpdateSettingsAsync(settings with
            {
                RestoreRoles = restoreRoles
            });

            await context.RespondAsync("I will " + (restoreRoles ? "now restore" : "no longer restore") + " a member's previous roles when they rejoin server.");
        }
    }
}
