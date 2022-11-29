using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class TempRoleModel : DatabaseTrackable<TempRoleModel>, IExpirable<TempRoleModel>
    {
        public ulong AssignerId { get; private set; }
        public ulong GuildId { get; private set; }
        public ulong UserId { get; private set; }
        public ulong RoleId { get; private set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset AssignedAt { get; private set; } = DateTimeOffset.UtcNow;

        public TempRoleModel() { }
        public TempRoleModel(ulong assignerId, ulong guildId, ulong userId, ulong roleId, DateTimeOffset expiresAt)
        {
            AssignerId = assignerId;
            GuildId = guildId;
            UserId = userId;
            RoleId = roleId;
            ExpiresAt = expiresAt;
        }

        public async Task<bool> ExpireAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ILogger<TempRoleModel> logger = serviceProvider.GetRequiredService<ILogger<TempRoleModel>>();
            DiscordShardedClient shardedClient = serviceProvider.GetRequiredService<DiscordShardedClient>();

            try
            {
                DiscordGuild guild;
                DiscordRole? role;
                DiscordMember? member;
                DiscordMember? assigner;

                try
                {
                    guild = await shardedClient.GetShard(GuildId).GetGuildAsync(GuildId);
                    if (guild.IsUnavailable)
                    {
                        logger.LogWarning("Guild {GuildId} is unavailable", GuildId);
                        return false;
                    }

                    role = guild.GetRole(RoleId);
                    member = await guild.GetMemberAsync(UserId);
                    assigner = await guild.GetMemberAsync(AssignerId);
                }
                catch (NotFoundException notFoundError)
                {
                    logger.LogDebug(notFoundError, "Failed to find guild, member or assigner for temp role {TempRoleId}. This likely means either the guild was deleted, or member/assigner left. Removing.", Id);
                    return true;
                }

                if (role == null)
                {
                    await member.SendMessageAsync($"Your temporary role <@&{RoleId}> ({Formatter.InlineCode(RoleId.ToString(CultureInfo.InvariantCulture))}) would have expired by now, however it seems it was deleted. Feel free to contact <@&{AssignerId}> ({Formatter.InlineCode(AssignerId.ToString(CultureInfo.InvariantCulture))}) for more information.");
                    await assigner.SendMessageAsync($"The temporary role <@&{RoleId}> ({Formatter.InlineCode(RoleId.ToString(CultureInfo.InvariantCulture))}) for {member.Mention} ({Formatter.InlineCode(UserId.ToString(CultureInfo.InvariantCulture))}) would have expired by now, however it seems it was deleted.");
                }
                else
                {
                    try
                    {
                        await member.RevokeRoleAsync(role);
                    }
                    catch (DiscordException roleError)
                    {
                        await member.SendMessageAsync($"Your temporary role {role.Mention} ({Formatter.InlineCode(RoleId.ToString(CultureInfo.InvariantCulture))}) would have expired by now, however I was unable to remove it. Feel free to contact <@&{AssignerId}> ({Formatter.InlineCode(AssignerId.ToString(CultureInfo.InvariantCulture))}) for more information.");
                        await assigner.SendMessageAsync($"The temporary role {role.Mention} ({Formatter.InlineCode(RoleId.ToString(CultureInfo.InvariantCulture))}) for {member.Mention} ({Formatter.InlineCode(UserId.ToString(CultureInfo.InvariantCulture))}) would have expired by now, however I was unable to remove it. HTTP Error {roleError.WebResponse.ResponseCode}: {roleError.JsonMessage}");
                        return true;
                    }
                    await member.SendMessageAsync($"Your temporary role {role.Mention} ({Formatter.InlineCode(RoleId.ToString(CultureInfo.InvariantCulture))}) has expired.");
                    await assigner.SendMessageAsync($"The temporary role {role.Mention} ({Formatter.InlineCode(RoleId.ToString(CultureInfo.InvariantCulture))}) for {member.Mention} ({Formatter.InlineCode(UserId.ToString(CultureInfo.InvariantCulture))}) has expired.");
                }
            }
            catch (ServerErrorException serverError)
            {
                logger.LogError(serverError, "Failed to remove temprole {Id} due to a server error. Role {RoleId} was not removed from member {UserId} in guild {GuildId}", Id, RoleId, UserId, GuildId);
                return false;
            }
            catch (DiscordException error)
            {
                logger.LogDebug(error, "Failed to remove temprole {Id} due to an unknown error. Role {RoleId} was not removed from member {UserId} in guild {GuildId}", Id, RoleId, UserId, GuildId);
                return false;
            }

            return true;
        }
    }
}
