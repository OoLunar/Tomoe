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
        /// Shows the current guild settings.
        /// </summary>
        [Command("show")]
        [RequirePermissions(DiscordPermissions.ManageGuild, DiscordPermissions.None)]
        public static async ValueTask ShowAsync(CommandContext context)
        {
            GuildSettingsModel? settings = await GuildSettingsModel.GetSettingsAsync(context.Guild!.Id);
            if (settings is null)
            {
                await context.RespondAsync(NOT_SETUP_TEXT);
                return;
            }

            DiscordEmbedBuilder embed = new();
            embed.WithTitle("Guild Settings");
            embed.AddField("Auto Dehoist", settings.AutoDehoist ? "Enabled" : "Disabled", true);
            embed.AddField("Auto Dehoist Format", settings.AutoDehoistFormat ?? "Dehoisted", true);
            embed.AddField("Restore Roles", settings.RestoreRoles ? "Enabled" : "Disabled", true);
            embed.AddField("Prefix", settings.TextPrefix ?? ">>", true);
            await context.RespondAsync(embed);
        }
    }
}
