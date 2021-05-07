namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class GuildMemberAdded
    {
        /// <summary>
        /// Attempts to make sure the user's roles persist. Will put the user into the database if they don't exist already.
        /// </summary>
        /// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
        /// <param name="eventArgs">Used to get the user id and to apply the roles to the user.</param>
        /// <returns></returns>
        public static async Task Handler(DiscordClient _client, GuildMemberAddEventArgs eventArgs)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guild = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
            if (guild != null)
            {
                GuildUser user = database.GuildUsers.FirstOrDefault(user => user.UserId == eventArgs.Member.Id && user.GuildId == eventArgs.Guild.Id);
                if (user != null)
                {
                    foreach (ulong roleId in user.Roles)
                    {
                        DiscordRole role = eventArgs.Guild.GetRole(roleId);
                        if (role != null)
                        {
                            await eventArgs.Member.GrantRoleAsync(role, "Persistent Roles.");
                        }
                    }
                }
                else
                {
                    user = new(eventArgs.Member.Id);
                    _ = database.GuildUsers.Add(user);
                    _ = await database.SaveChangesAsync();
                }
            }
        }
    }
}
