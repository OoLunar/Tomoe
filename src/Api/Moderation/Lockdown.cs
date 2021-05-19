namespace Tomoe.Api
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public class Lockdown
        {
            public enum LockdownType
            {
                Channel,
                Server,
                Bots,
                Role
            }

            public static async Task Channel(DiscordGuild discordGuild, bool recordToModLog = true, ulong discordUserId = 0, Database database = null, List<DiscordChannel> discordChannels = null, List<DiscordRole> discordRoles = null, string lockReason = Constants.MissingReason)
            {
                bool saveToDatabase = database == null;
                if (database == null)
                {
                    IServiceScope scope = Program.ServiceProvider.CreateScope();
                    database = scope.ServiceProvider.GetService<Database>();
                }

                List<Lock> localLocks = new();

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

                foreach (DiscordChannel discordChannel in discordChannels)
                {
                    foreach (DiscordRole discordRole in discordRoles)
                    {
                        if (!discordRole.HasPermission(Permissions.SendMessages) && !discordRole.HasPermission(Permissions.AddReactions))
                        {
                            continue;
                        }

                        DiscordOverwrite discordChannelOverwrite = discordChannel.PermissionOverwrites.FirstOrDefault(overwrite => overwrite.Id == discordRole.Id);
                        Permissions discordChannelPermissions = discordChannel.Type switch
                        {
                            ChannelType.Category => Permissions.SendMessages | Permissions.AddReactions | Permissions.UseVoice,
                            ChannelType.Voice => Permissions.UseVoice,
                            ChannelType.Text => Permissions.SendMessages | Permissions.AddReactions,
                            ChannelType.News => Permissions.SendMessages | Permissions.AddReactions,
                            ChannelType.Private => throw new NotImplementedException(),
                            ChannelType.Group => throw new NotImplementedException(),
                            ChannelType.Store => throw new NotImplementedException(),
                            ChannelType.Unknown => throw new NotImplementedException(),
                            _ => Permissions.SendMessages | Permissions.AddReactions | Permissions.UseVoice
                        };

                        Lock localLock = new();
                        localLock.GuildId = discordGuild.Id;
                        localLock.ChannelId = discordChannel.Id;
                        localLock.RoleId = discordRole.Id;
                        if (discordChannelOverwrite == null)
                        {
                            localLock.HadPreviousOverwrite = false;
                        }
                        else
                        {
                            localLock.HadPreviousOverwrite = true;
                            localLock.Allowed = discordChannelOverwrite.Allowed;
                            localLock.Denied = discordChannelOverwrite.Denied;
                        }

                        localLocks.Add(localLock);
                        await discordChannel.AddOverwriteAsync(discordRole, discordChannelOverwrite.Allowed, discordChannelOverwrite.Denied.Grant(discordChannelPermissions), "Channel lockdown");
                    }
                }

                database.Locks.AddRange(localLocks);
                if (recordToModLog)
                {
                    await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> locked roles {string.Join(", ", discordRoles.Select(role => role.Mention))} in channels {string.Join(", ", discordChannels.Select(channel => channel.Mention))}: {lockReason}");
                }

                if (saveToDatabase)
                {
                    await database.SaveChangesAsync();
                    await database.DisposeAsync();
                }
            }

            public static async Task Server(DiscordGuild discordGuild, ulong discordUserId, string lockReason = Constants.MissingReason, params DiscordRole[] discordRoles)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                await Channel(discordGuild, false, discordUserId, database, null, discordRoles.Length == 0 ? null : discordRoles.ToList(), lockReason);
                await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> locked the server: {lockReason}");
                await database.SaveChangesAsync();
            }

            public static async Task Bots(DiscordGuild discordGuild, ulong discordUserId, string lockReason = Constants.MissingReason, params DiscordChannel[] discordChannels)
            {
                List<DiscordRole> roles = discordGuild.Roles.Values.Where(role => role.IsManaged).ToList();
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                await Channel(discordGuild, false, discordUserId, database, discordChannels.Length == 0 ? null : new(discordChannels), null, lockReason);
                await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> locked bots {(discordChannels.Length == 0 ? "across the server" : $"in channels {string.Join(", ", discordChannels.Select(channel => channel.Mention))}")}: {lockReason}");
                await database.SaveChangesAsync();
            }

            public static async Task Role(DiscordGuild discordGuild, DiscordRole discordRole, ulong discordUserId, string lockReason = Constants.MissingReason)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                await Channel(discordGuild, false, discordUserId, database, null, new() { discordRole }, lockReason);
                await ModLog(discordGuild, LogType.Lock, database, $"<@{discordUserId}> has locked the role {discordRole.Mention} across the server: {lockReason}");
                await database.SaveChangesAsync();
            }
        }
    }
}