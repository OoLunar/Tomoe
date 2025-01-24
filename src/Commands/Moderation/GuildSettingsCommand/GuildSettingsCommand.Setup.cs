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
        /// Configures the guild settings.
        /// </summary>
        /// <param name="autoDehoistEnabled">Whether or not to enable auto-dehoist.</param>
        /// <param name="autoDehoistFormat">The format to use when auto-dehoisting.</param>
        /// <param name="restoreRoles">Whether or not to restore roles when a member rejoins the server.</param>
        /// <param name="textPrefix">The prefix to use for text commands.</param>
        [Command("setup")]
        [RequirePermissions([DiscordPermission.ManageNicknames], [])]
        public static async ValueTask SetupAsync(CommandContext context, bool autoDehoistEnabled = false, string? autoDehoistFormat = null, bool restoreRoles = false, string? textPrefix = null)
        {
            GuildSettingsModel settings = new()
            {
                GuildId = context.Guild!.Id,
                AutoDehoist = autoDehoistEnabled,
                AutoDehoistFormat = autoDehoistFormat,
                RestoreRoles = restoreRoles,
                TextPrefix = textPrefix
            };

            await GuildSettingsModel.UpdateSettingsAsync(settings);
            await context.RespondAsync("Guild settings have been updated.");
        }
    }
}
