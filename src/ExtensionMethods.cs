namespace Tomoe
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ExtensionMethods
    {
        /// <summary>
        /// Attempts to retrieve the DiscordMember from cache, then the API if the cache does not have the member.
        /// </summary>
        /// <param name="discordGuild">The guild to get the DiscordMember from.</param>
        /// <param name="discordUserId">The id to search for in the DiscordGuild.</param>
        /// <returns>The DiscordMember from the DiscordGuild</returns>
        public static async Task<DiscordMember> GetMember(this ulong discordUserId, DiscordGuild discordGuild)
        {
            try
            {
                return discordGuild.Members.Values.FirstOrDefault(member => member.Id == discordUserId) ?? await discordGuild.GetMemberAsync(discordUserId);
            }
            catch (NotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                // Exceptions are not our problem
                throw;
            }
        }

        public static async Task<bool> TryDmMember(this DiscordUser discordUser, string message)
        {
            bool sentDm = false;
            if (discordUser != null && !discordUser.IsBot)
            {
                foreach (DiscordGuild discordGuild in Program.Client.Guilds.Values)
                {
                    try
                    {
                        DiscordMember discordMember = await discordGuild.GetMemberAsync(discordUser.Id);
                        await (await discordMember.CreateDmChannelAsync()).SendMessageAsync(message);
                        sentDm = true;
                        break;
                    }
                    catch (NotFoundException) { }
                    catch (UnauthorizedException) { }
                }
            }
            return sentDm;
        }

        public static DiscordRole GetRole(this ulong roleId, DiscordGuild guild) => roleId != 0 ? guild.GetRole(roleId) : null;
        public static bool HasPermission(this DiscordMember guildMember, Permissions permission) => !guildMember.Roles.Any() ? guildMember.Guild.EveryoneRole.Permissions.HasPermission(permission) : guildMember.Roles.Any(role => role.Permissions.HasPermission(permission));
    }
}
