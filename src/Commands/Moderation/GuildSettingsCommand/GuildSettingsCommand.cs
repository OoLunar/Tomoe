using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Manages the guild settings.
    /// </summary>
    [Command("guild_settings"), TextAlias("guild_config"), RequirePermissions([], [DiscordPermission.ManageGuild])]
    public sealed partial class GuildSettingsCommand
    {
        private const string NOT_SETUP_TEXT = "The guild settings have not been setup yet. Please run `/guild_settings setup` to configure the guild settings.";
    }
}
