namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.EventArgs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class GuildMemberRemoved
    {
        /// <summary>
        ///	Stores the users roles before they leave in case they wish to rejoin.
        /// </summary>
        /// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
        /// <param name="eventArgs">Used to get the roles.</param>
        /// <returns></returns>
        public static async Task Handler(DiscordClient _client, GuildMemberRemoveEventArgs eventArgs)
        {
            GuildDownloadCompleted.MemberCount[eventArgs.Guild.Id]--;
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
            if (guildConfig != null)
            {
                GuildUser guildUser = database.GuildUsers.FirstOrDefault(user => user.UserId == eventArgs.Member.Id && user.GuildId == eventArgs.Guild.Id);
                if (guildUser != null)
                {
                    guildUser.Roles.AddRange(eventArgs.Member.Roles.Except(new[] { eventArgs.Guild.EveryoneRole }).Select(role => role.Id));
                }
            }
        }
    }
}
