using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
    public class GuildAvailable
    {
        private static readonly ILogger _logger = Log.ForContext<GuildAvailable>();

        /// <summary>
        /// Used to add the guild to the database and log when the guild is available.
        /// </summary>
        /// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
        /// <param name="eventArgs">Used to get the guild id and guild name.</param>
        public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
            if (guildConfig == null)
            {
                guildConfig = new(eventArgs.Guild.Id);
                database.GuildConfigs.Add(guildConfig);
                await database.SaveChangesAsync();
            }

            // Find new users by removing the database's current user list's through id's
            List<ulong> newGuildIds = eventArgs.Guild.Members.Keys.Except(database.GuildUsers.Where(guildUser => guildUser.GuildId == eventArgs.Guild.Id).Select(guildUser => guildUser.UserId)).ToList();
            List<GuildUser> updatedGuildUsers = new();
            foreach (ulong memberId in newGuildIds)
            {
                DiscordMember member = eventArgs.Guild.Members[memberId];
                GuildUser guildUser = new(memberId);
                guildUser.GuildId = eventArgs.Guild.Id;
                guildUser.Roles.AddRange(member.Roles.Except(new[] { eventArgs.Guild.EveryoneRole }).Select(role => role.Id));
                guildUser.JoinedAt = member.JoinedAt.DateTime;
                updatedGuildUsers.Add(guildUser);
            }
            // If the updated guild user count isn't zero, save the new users to the database. An exception will be thrown if nothing new is added.
            if (updatedGuildUsers.Count != 0)
            {
                database.GuildUsers.AddRange(updatedGuildUsers);
                await database.SaveChangesAsync();
            }

            GuildDownloadCompleted.MemberCount[eventArgs.Guild.Id] = eventArgs.Guild.MemberCount;
            _logger.Information($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
        }
    }
}