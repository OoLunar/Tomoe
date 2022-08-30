using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace OoLunar.Tomoe.Utilities
{
    public static class ExtensionMethods
    {
        public static async Task<Optional<DiscordMessage?>> GetMessageAsync(this DiscordShardedClient shardedClient, ulong channelId, ulong messageId, ulong? guildId = null)
        {
            ArgumentNullException.ThrowIfNull(shardedClient, nameof(shardedClient));

            try
            {
                // We don't do GetShard(guildId ?? 0).GetChannelAsync here because we want to check if the guild is unavailable
                DiscordChannel channel;
                if (guildId == null)
                {
                    channel = await shardedClient.GetShard(0).GetChannelAsync(channelId);
                }
                else
                {
                    // Rest request here since you can not trust the D#+ cache. Also the message might be in a guild that isn't cached (though highly unlikely).
                    DiscordGuild guild = await shardedClient.GetShard(guildId.Value).GetGuildAsync(guildId.Value);
                    if (guild.IsUnavailable)
                    {
                        return new Optional<DiscordMessage?>(null);
                    }

                    channel = guild.GetChannel(channelId);
                }

                if (channel != null)
                {
                    return new Optional<DiscordMessage?>(await channel.GetMessageAsync(messageId));
                }
            }
            catch (DiscordException error)
            {
                return error.WebResponse.ResponseCode >= 500
                    ? new Optional<DiscordMessage?>(null) // Server problem
                    : Optional.FromNoValue<DiscordMessage?>(); // Permissions problem
            }

            return Optional.FromNoValue<DiscordMessage?>();
        }

        /// <summary>
		/// Checks to see if memberA can execute X action on memberB. Runs the following checks: If memberA is the guild owner, if memberB is the guild owner, if memberA has the proper permissions to execute X action, if memberB's hierarchy isn't higher than memberA's hierarchy.
		/// </summary>
		/// <param name="memberA">Which user is executing the action.</param>
		/// <param name="permissions">Which permission is associated with the action.</param>
		/// <param name="memberB">Who's being affected.</param>
		/// <returns>Whether memberA can execute X action on memberB.</returns>
		public static bool CanExecute(this DiscordMember memberA, Permissions permissions, DiscordMember memberB)
        {
            ArgumentNullException.ThrowIfNull(memberA, nameof(memberA));
            ArgumentNullException.ThrowIfNull(memberB, nameof(memberB));
            return memberA.IsOwner || (!memberB.IsOwner && memberA.Permissions.HasPermission(permissions) && memberB.Hierarchy < memberA.Hierarchy);
        }
    }
}
