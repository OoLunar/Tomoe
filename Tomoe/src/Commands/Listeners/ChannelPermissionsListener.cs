using System.Linq;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Models;

namespace Tomoe.Commands
{
    public sealed class ChannelPermissionsListener
    {
        public static void ChannelPermissions(DiscordClient discordClient, ChannelCreateEventArgs channelCreateEventArgs)
        {
            if (channelCreateEventArgs.Guild is null)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetRequiredService<Database>();
            GuildConfig guildConfig = database.GuildConfigs.First(databaseGuildConfig => databaseGuildConfig.Id == channelCreateEventArgs.Guild.Id);
        }
    }
}