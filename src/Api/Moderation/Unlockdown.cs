namespace Tomoe.Api
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public class Unlockdown
        {
            public static async Task Channel(DiscordGuild discordGuild, bool recordToModLog = true, ulong discordUserId = 0, Database database = null, List<DiscordChannel> discordChannels = null, List<DiscordRole> discordRoles = null, string unlockReason = Constants.MissingReason)
            {
                bool saveToDatabase = database == null;
                if (database == null)
                {
                    IServiceScope scope = Program.ServiceProvider.CreateScope();
                    database = scope.ServiceProvider.GetService<Database>();
                }

                if (discordRoles == null)
                {
                    discordRoles = new();
                    discordRoles.AddRange(discordGuild.Roles.Values);
                }

                if (discordChannels == null)
                {
                    discordChannels = new();
                    discordChannels.AddRange(discordGuild.Channels.Values);
                }

                List<Lock> databaseLocks = database.Locks.Where(dbLock => dbLock.GuildId == discordGuild.Id && (discordRoles.Select(role => role.Id).Contains(dbLock.RoleId) || discordChannels.Select(channel => channel.Id).Contains(dbLock.ChannelId))).Distinct().ToList();

                foreach (Lock databaseLock in databaseLocks)
                {
                    DiscordChannel discordChannel = discordGuild.GetChannel(databaseLock.ChannelId);
                    if (databaseLock.HadPreviousOverwrite)
                    {
                        await discordChannel.AddOverwriteAsync(databaseLock.RoleId.GetRole(discordGuild), databaseLock.Allowed, databaseLock.Denied, "Unlocking channel to previous overwrites.");
                    }
                    else
                    {
                        DiscordOverwrite discordChannelOverwrite = discordChannel.PermissionOverwrites.FirstOrDefault(overwrite => overwrite.Id == databaseLock.RoleId);
                        if (discordChannelOverwrite != null)
                        {
                            await discordChannelOverwrite.DeleteAsync("Unlocking channel to previous overwrites");
                        }
                    }
                }
                database.Locks.RemoveRange(databaseLocks);

                if (recordToModLog)
                {
                    await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> unlocked roles {string.Join(", ", discordRoles.Select(role => role.Mention))} in channels {string.Join(", ", discordChannels.Select(channel => channel.Mention))}: {unlockReason}");
                }

                if (saveToDatabase)
                {
                    await database.SaveChangesAsync();
                    await database.DisposeAsync();
                }
            }

            public static async Task Server(DiscordGuild discordGuild, ulong discordUserId, string unlockReason = Constants.MissingReason, params DiscordRole[] discordRoles)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                await Channel(discordGuild, false, discordUserId, database, null, discordRoles.Length == 0 ? null : discordRoles.ToList(), unlockReason);
                await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> unlocked the server: {unlockReason}");
                await database.SaveChangesAsync();
            }

            public static async Task Bots(DiscordGuild discordGuild, ulong discordUserId, string unlockReason = Constants.MissingReason, params DiscordChannel[] discordChannels)
            {
                List<DiscordRole> roles = discordGuild.Roles.Values.Where(role => role.IsManaged).ToList();
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                await Channel(discordGuild, false, discordUserId, database, discordChannels.Length == 0 ? null : new(discordChannels), null, unlockReason);
                await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> unlocked bots {(discordChannels.Length == 0 ? "across the server" : $"in channels {string.Join(", ", discordChannels.Select(channel => channel.Mention))}")}: {unlockReason}");
                await database.SaveChangesAsync();
            }

            public static async Task Role(DiscordGuild discordGuild, DiscordRole discordRole, ulong discordUserId, string unlockReason = Constants.MissingReason)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                await Channel(discordGuild, false, discordUserId, database, null, new() { discordRole }, unlockReason);
                await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> has unlocked the role {discordRole.Mention} across the server: {unlockReason}");
                await database.SaveChangesAsync();
            }
        }
    }
}