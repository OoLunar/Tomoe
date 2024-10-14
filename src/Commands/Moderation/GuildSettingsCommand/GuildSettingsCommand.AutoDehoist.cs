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
        /// Controls whether the bot should automatically dehoist users with hoisted names.
        /// </summary>
        /// <remarks>
        /// A hoisted name is a name that starts with a special character, such
        /// as an exclamation mark or a tilde that causes the user to appear at
        /// the top of the member list. It's mainly used for advertising or
        /// trolling purposes.
        /// </remarks>
        /// <param name="enabled">Whether the bot should automatically dehoist users.</param>
        /// <param name="format">The format to use when dehoisting users. Available variables are `display_name` and `user_name`.</param>
        [Command("auto_dehoist")]
        [RequirePermissions(DiscordPermissions.ManageNicknames, DiscordPermissions.ManageGuild)]
        public static async ValueTask AutoDehoistAsync(CommandContext context, bool enabled, string? format = null)
        {
            // Check to see if the command has been setup.
            GuildSettingsModel? settings = await GuildSettingsModel.GetSettingsAsync(context.Guild!.Id);
            if (settings is null)
            {
                await context.RespondAsync(NOT_SETUP_TEXT);
                return;
            }

            await GuildSettingsModel.UpdateSettingsAsync(settings with
            {
                AutoDehoist = enabled,
                AutoDehoistFormat = format
            });

            await context.RespondAsync($"Auto dehoist has been {(enabled ? "enabled" : "disabled")}.");
        }
    }
}
