namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using Tomoe.Db;

    public class ChannelPermissions
    {
        public static void Handler(DiscordClient discordClient, ChannelCreateEventArgs channelCreateEventArgs)
        {
            if (channelCreateEventArgs.Guild == null)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = database.GuildConfigs.First(databaseGuildConfig => databaseGuildConfig.Id == channelCreateEventArgs.Guild.Id);
        }
    }
}
