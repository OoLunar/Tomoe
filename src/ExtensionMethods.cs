namespace Tomoe
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ExtensionMethods
    {
        public static DiscordEmbedBuilder GenerateDefaultEmbed(this DiscordEmbedBuilder embedBuilder, CommandContext context, string title = null)
        {
            if (!string.IsNullOrEmpty(title))
            {
                embedBuilder.Title = title.Titleize();
            }

            embedBuilder.Color = new DiscordColor("#7b84d1");
            embedBuilder.Author = new()
            {
                Name = context.Guild == null ? context.User.Username : context.Member.DisplayName,
                IconUrl = context.User.AvatarUrl,
                Url = context.User.AvatarUrl
            };
            return embedBuilder;
        }

        public static DiscordEmbedBuilder GenerateDefaultEmbed(this DiscordEmbedBuilder embedBuilder, InteractionContext context, string title = null)
        {
            if (!string.IsNullOrEmpty(title))
            {
                embedBuilder.Title = title.Titleize();
            }

            embedBuilder.Color = new DiscordColor("#7b84d1");
            embedBuilder.Author = new()
            {
                Name = context.Guild == null ? context.User.Username : context.Member.DisplayName,
                IconUrl = context.User.AvatarUrl,
                Url = context.User.AvatarUrl
            };
            return embedBuilder;
        }

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
            // TODO: Get shared servers and try dming the member through there when dm's are off.
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
        public static bool HasPermission(this DiscordMember guildMember, Permissions permission) => !guildMember.Roles.Any() ? guildMember.Guild.EveryoneRole.HasPermission(permission) : guildMember.Roles.Any(role => role.HasPermission(permission));
        public static bool HasPermission(this DiscordRole role, Permissions permission) => role.Permissions.HasPermission(permission);
    }
}
