using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed partial class GuildSettingsCommand
    {
        private readonly GuildPrefixResolver _guildPrefixResolver;

        /// <summary>
        /// Creates a new <see cref="GuildSettingsCommand"/> with the provided <see cref="GuildPrefixResolver"/>.
        /// </summary>
        /// <param name="guildPrefixResolver">The <see cref="GuildPrefixResolver"/> to use for changing the prefix.</param>
        public GuildSettingsCommand(IPrefixResolver guildPrefixResolver)
        {
            if (guildPrefixResolver is not GuildPrefixResolver resolver)
            {
                throw new ArgumentException($"The prefix resolver must be of type {nameof(GuildPrefixResolver)}.");
            }

            _guildPrefixResolver = resolver;
        }

        /// <summary>
        /// Changes the text prefix that the bot uses in this server.
        /// </summary>
        /// <param name="prefix">The new text prefix.</param>
        [Command("text_prefix")]
        [RequirePermissions(DiscordPermissions.None, DiscordPermissions.ManageGuild)]
        public async ValueTask TextPrefixAsync(CommandContext context, string prefix)
        {
            GuildSettingsModel? settings = await GuildSettingsModel.GetSettingsAsync(context.Guild!.Id);
            if (settings is null)
            {
                await context.RespondAsync(NOT_SETUP_TEXT);
                return;
            }

            prefix = prefix.Trim();
            await GuildSettingsModel.UpdateSettingsAsync(settings with
            {
                TextPrefix = prefix
            });

            _guildPrefixResolver.ChangePrefix(context.Guild!.Id, prefix);
            await context.RespondAsync($"The text prefix has been set to `{prefix}`.");
        }
    }
}
