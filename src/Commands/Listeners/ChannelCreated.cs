namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using static Tomoe.Api.Moderation;

    public class ChannelCreated
    {
        /// <summary>
        /// Overrides channel permissions for the guild's punishment roles, if configured.
        /// </summary>
        /// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
        /// <param name="eventArgs">ChannelCreateEventArgs that are used to retrieve the guild and the channel.</param>
        /// <returns></returns>
        public static async Task Handler(DiscordClient _client, ChannelCreateEventArgs eventArgs)
        {
            if (eventArgs.Guild == null)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
            if (guildConfig != null)
            {
                DiscordRole muteRole = guildConfig.MuteRole.GetRole(eventArgs.Guild);
                if (muteRole != null)
                {
                    await Moderation.Config.FixPermissions(eventArgs.Channel, RoleAction.Mute, muteRole);
                }

                DiscordRole antimemeRole = guildConfig.AntimemeRole.GetRole(eventArgs.Guild);
                if (antimemeRole != null)
                {
                    await Moderation.Config.FixPermissions(eventArgs.Channel, RoleAction.Antimeme, antimemeRole);
                }

                DiscordRole voicebanRole = guildConfig.VoicebanRole.GetRole(eventArgs.Guild);
                if (voicebanRole != null)
                {
                    await Moderation.Config.FixPermissions(eventArgs.Channel, RoleAction.Voiceban, voicebanRole);
                }
            }
        }
    }
}
