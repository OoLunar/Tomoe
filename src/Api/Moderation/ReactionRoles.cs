namespace Tomoe.Api
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public class ReactionRoles
        {
            public static Dictionary<DiscordEmoji, DiscordRole> Parse(DiscordClient discordClient, DiscordGuild discordGuild, string emojiRoleString)
            {
                List<string> emojiRoleList = emojiRoleString.Replace('\n', ' ').Split(' ').ToList();
                if (emojiRoleList.Count % 2 == 1)
                {
                    throw new ArgumentException("Missing partner in emoji->role pair.");
                }

                Dictionary<DiscordEmoji, DiscordRole> emojiRoleDictionary = new();
                for (int i = 0; i < emojiRoleList.Count / 2; i += 2)
                {
                    DiscordEmoji discordEmoji = emojiRoleList[i] switch
                    {
                        _ when DiscordEmoji.TryFromName(discordClient, emojiRoleList[i], out DiscordEmoji emoji) => emoji,
                        _ when DiscordEmoji.TryFromUnicode(emojiRoleList[i], out DiscordEmoji emoji) => emoji,
                        _ when DiscordEmoji.TryFromGuildEmote(discordClient, ulong.Parse(emojiRoleList[i]), out DiscordEmoji emoji) => emoji,
                        _ => throw new ArgumentException("Not an emoji")
                    };

                    if (!ulong.TryParse(emojiRoleList[i + 1], NumberStyles.Number, CultureInfo.InvariantCulture, out ulong roleId))
                    {
                        // When manually pinging the role instead of providing the role id
                        string stringRoleId = emojiRoleList[i + 1][3..^1];
                        if (!ulong.TryParse(stringRoleId, NumberStyles.Number, CultureInfo.InvariantCulture, out roleId))
                        {
                            throw new ArgumentException($"{emojiRoleList[i + 1]} is not a role or role id.");
                        }
                    }

                    DiscordRole discordRole = discordGuild.GetRole(roleId);
                    emojiRoleDictionary[discordEmoji] = discordRole ?? throw new ArgumentException($"Discord role {roleId} doesn't exist in guild {discordGuild.Id}!");
                }

                return emojiRoleDictionary;
            }

            public static async Task<bool> Create(DiscordClient discordClient, DiscordGuild discordGuild, ulong discordUserId, DiscordMessage discordMessage, string emojiRoleString)
            {
                Dictionary<DiscordEmoji, DiscordRole> discordReactionRoles = Parse(discordClient, discordGuild, emojiRoleString);
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                bool reactionRolesCreated = false;
                foreach ((DiscordEmoji discordEmoji, DiscordRole discordRole) in discordReactionRoles)
                {
                    ReactionRole reactionRole = database.ReactionRoles.FirstOrDefault(reactionRole => reactionRole.EmojiName == discordEmoji.GetDiscordName() && reactionRole.RoleId == discordRole.Id);
                    if (reactionRole != null)
                    {
                        continue;
                    }
                    reactionRole = new();
                    reactionRole.ChannelId = discordMessage.ChannelId;
                    reactionRole.EmojiName = discordEmoji.GetDiscordName();
                    reactionRole.GuildId = discordGuild.Id;
                    reactionRole.MessageId = discordMessage.Id;
                    reactionRole.RoleId = discordRole.Id;
                    database.ReactionRoles.Add(reactionRole);
                    reactionRolesCreated = true;
                }
                await ModLog(discordGuild, LogType.ReactionRoleCreate, database, $"<@{discordUserId}> created the following reaction roles: {string.Join(", ", discordReactionRoles.Select(reactionRole => $"{reactionRole.Key} => {reactionRole.Value.Mention}"))}");
                await database.SaveChangesAsync();
                await Fix(discordClient, discordGuild, discordUserId, discordMessage);
                return reactionRolesCreated;
            }

            public static async Task<bool> Delete(DiscordClient discordClient, DiscordGuild discordGuild, ulong discordUserId, DiscordMessage discordMessage, string emojiRoleString)
            {
                Dictionary<DiscordEmoji, DiscordRole> discordReactionRoles = Parse(discordClient, discordGuild, emojiRoleString);
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                bool reactionRolesCreated = false;
                foreach ((DiscordEmoji discordEmoji, DiscordRole discordRole) in discordReactionRoles)
                {
                    ReactionRole reactionRole = database.ReactionRoles.FirstOrDefault(reactionRole => reactionRole.EmojiName == discordEmoji.GetDiscordName() && reactionRole.RoleId == discordRole.Id);
                    if (reactionRole != null)
                    {
                        await discordMessage.DeleteOwnReactionAsync(DiscordEmoji.FromName(discordClient, reactionRole.EmojiName, true));
                        database.ReactionRoles.Remove(reactionRole);
                        reactionRolesCreated = true;
                    }
                }
                await ModLog(discordGuild, LogType.ReactionRoleDelete, database, $"<@{discordUserId}> deleted the following reaction roles: {string.Join(", ", discordReactionRoles.Select(reactionRole => $"{reactionRole.Key} => {reactionRole.Value.Mention}"))}");
                await database.SaveChangesAsync();
                return reactionRolesCreated;
            }

            public static async Task<bool> Fix(DiscordClient client, DiscordGuild discordGuild, ulong discordUserId, DiscordChannel discordChannel)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                List<ReactionRole> databaseReactionRoles = database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == discordGuild.Id && reactionRole.ChannelId == discordChannel.Id).ToList();

                if (databaseReactionRoles.Count == 0)
                {
                    return false;
                }

                bool reactionRolesFixed = false;
                int totalRoleAssignments = 0;

                foreach (ReactionRole databaseReactionRole in databaseReactionRoles)
                {
                    DiscordMessage discordMessage = null;
                    try
                    {
                        discordMessage = await discordChannel.GetMessageAsync(databaseReactionRole.MessageId);
                    }
                    catch (Exception) { }
                    DiscordEmoji discordEmoji = DiscordEmoji.FromName(client, databaseReactionRole.EmojiName, true);
                    DiscordRole discordRole = discordGuild.GetRole(databaseReactionRole.RoleId);
                    if (discordRole == null || discordMessage == null)
                    {
                        database.ReactionRoles.Remove(databaseReactionRole);
                        continue;
                    }

                    reactionRolesFixed = true;
                    await discordMessage.CreateReactionAsync(discordEmoji);
                    foreach (DiscordUser discordReactor in await discordMessage.GetReactionsAsync(discordEmoji, 100000))
                    {
                        DiscordMember discordMember = await discordReactor.Id.GetMember(discordGuild);
                        if (discordMember != null && !discordMember.Roles.Contains(discordRole))
                        {
                            await discordMember.GrantRoleAsync(discordRole);
                            totalRoleAssignments++;
                        }
                    }
                }

                await ModLog(discordGuild, LogType.ReactionRoleDelete, database, $"<@{discordUserId}> fixed all reaction roles on channe {discordChannel.Mention}. Assigned a total of {totalRoleAssignments.ToMetric()} roles.");
                await database.SaveChangesAsync();
                return reactionRolesFixed;
            }

            public static async Task<bool> Fix(DiscordClient client, DiscordGuild discordGuild, ulong discordUserId, DiscordMessage discordMessage)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                List<ReactionRole> databaseReactionRoles = database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == discordGuild.Id && reactionRole.ChannelId == discordMessage.ChannelId && reactionRole.MessageId == discordMessage.Id).ToList();

                if (databaseReactionRoles.Count == 0)
                {
                    return false;
                }

                bool reactionRolesFixed = false;
                int totalRoleAssignments = 0;

                foreach (ReactionRole databaseReactionRole in databaseReactionRoles)
                {
                    DiscordEmoji discordEmoji = DiscordEmoji.FromName(client, databaseReactionRole.EmojiName, true);
                    DiscordRole discordRole = discordGuild.GetRole(databaseReactionRole.RoleId);
                    if (discordEmoji == null || discordRole == null)
                    {
                        database.ReactionRoles.Remove(databaseReactionRole);
                        continue;
                    }

                    reactionRolesFixed = true;
                    await discordMessage.CreateReactionAsync(discordEmoji);
                    foreach (DiscordUser discordReactor in await discordMessage.GetReactionsAsync(discordEmoji, 100000))
                    {
                        DiscordMember discordMember = await discordReactor.Id.GetMember(discordGuild);
                        if (discordMember != null && !discordMember.Roles.Contains(discordRole))
                        {
                            await discordMember.GrantRoleAsync(discordRole);
                            totalRoleAssignments++;
                        }
                    }
                }

                await ModLog(discordGuild, LogType.ReactionRoleDelete, database, $"<@{discordUserId}> fixed all reaction roles on message {discordMessage.JumpLink}. Assigned a total of {totalRoleAssignments.ToMetric()} roles.");
                await database.SaveChangesAsync();
                return reactionRolesFixed;
            }

            public static async Task<bool> Fix(DiscordClient client, DiscordGuild discordGuild, ulong discordUserId, DiscordMessage discordMessage, DiscordEmoji discordEmoji)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                ReactionRole databaseReactionRoles = database.ReactionRoles.FirstOrDefault(reactionRole => reactionRole.GuildId == discordGuild.Id && reactionRole.ChannelId == discordMessage.ChannelId && reactionRole.MessageId == discordMessage.Id && reactionRole.EmojiName == discordEmoji.GetDiscordName());

                if (databaseReactionRoles == null)
                {
                    return false;
                }

                bool reactionRolesFixed = false;
                int totalRoleAssignments = 0;

                DiscordRole discordRole = discordGuild.GetRole(databaseReactionRoles.RoleId);
                if (discordRole == null)
                {
                    database.ReactionRoles.Remove(databaseReactionRoles);
                    await database.SaveChangesAsync();
                    return false;
                }

                reactionRolesFixed = true;
                await discordMessage.CreateReactionAsync(discordEmoji);
                foreach (DiscordUser discordReactor in await discordMessage.GetReactionsAsync(discordEmoji, 100000))
                {
                    DiscordMember discordMember = await discordReactor.Id.GetMember(discordGuild);
                    if (discordMember != null && !discordMember.Roles.Contains(discordRole))
                    {
                        await discordMember.GrantRoleAsync(discordRole);
                        totalRoleAssignments++;
                    }
                }

                await ModLog(discordGuild, LogType.ReactionRoleDelete, database, $"<@{discordUserId}> fixed reaction role {$"{databaseReactionRoles.EmojiName} => <@&{databaseReactionRoles.RoleId}>"} on message {discordMessage.JumpLink}. Assigned the role to {totalRoleAssignments.ToMetric()} people.");
                await database.SaveChangesAsync();
                return reactionRolesFixed;
            }
            public static List<ReactionRole> Get(ulong discordGuildId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == discordGuildId).ToList();
            }

            public static List<ReactionRole> Get(ulong discordGuildId, DiscordChannel discordChannel)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == discordGuildId && reactionRole.ChannelId == discordChannel.Id).ToList();
            }

            public static List<ReactionRole> Get(ulong discordGuildId, DiscordChannel discordChannel, ulong discordMessageId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == discordGuildId && reactionRole.ChannelId == discordChannel.Id && reactionRole.MessageId == discordMessageId).ToList();
            }

            public static List<ReactionRole> Get(ulong discordGuildId, ulong discordRoleId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == discordGuildId && reactionRole.RoleId == discordRoleId).ToList();
            }

            public static List<ReactionRole> Get(ulong discordGuildId, string discordEmojiName)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == discordGuildId && reactionRole.EmojiName == discordEmojiName).ToList();
            }
        }
    }
}