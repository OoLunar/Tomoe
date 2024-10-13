using DSharpPlus.Commands;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Manages the guild settings.
    /// </summary>
    [Command("guild_settings")]
    public sealed partial class GuildSettingsCommand
    {
        private const string NOT_SETUP_TEXT = "The guild settings have not been setup yet. Please run `/guild_settings setup` to configure the guild settings.";
    }
}
