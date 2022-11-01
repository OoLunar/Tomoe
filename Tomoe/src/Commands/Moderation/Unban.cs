using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Commands.Attributes;

namespace Tomoe.Commands.Moderation
{
    public sealed class Unban : ApplicationCommandModule
    {
        [SlashCommand("unban", "Unbans a person from the guild."), Hierarchy(Permissions.BanMembers)]
        public static async Task UnbanAsync(InteractionContext context, [Option("victim_id", "The Discord user id of who to unban from the guild.")] string victimIdString, [Option("reason", "Why is the victim being unbanned from the guild.")] string unbanReason = Constants.MissingReason)
        {
            if (!ulong.TryParse(victimIdString, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong victimId))
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: Unknown user id {Formatter.InlineCode(victimIdString)}. To get a user's id, enable Developer Mode on the Discord Client, right click the user you banned, and at the bottom of the menu, click \"Copy ID\""
                });
                return;
            }

            DiscordUser victim = await context.Client.GetUserAsync(victimId);
            if (victim == null)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: <@{victimId}> ({victimId}) is not a Discord user!"
                });
                return;
            }

            try
            {
                DiscordBan discordBan = await context.Guild.GetBanAsync(victimId);
            }
            catch (NotFoundException)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: <@{victimId}> is not banned!"
                });
                return;
            }

            await context.Guild.UnbanMemberAsync(victim.Id, unbanReason);
            bool sentDm = await victim.TryDmMemberAsync($"You've been unbanned from {context.Guild.Name} by {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}). Reason: {unbanReason}");

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", context.Guild.Name },
                { "guild_count", Program.TotalMemberCount[context.Guild.Id].ToMetric() },
                { "guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_username", victim.Username },
                { "victim_tag", victim.Discriminator },
                { "victim_mention", victim.Mention },
                { "victim_id", victim.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_username", context.Member.Username },
                { "moderator_tag", context.Member.Discriminator },
                { "moderator_mention", context.Member.Mention },
                { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_displayname", context.Member.DisplayName },
                { "punishment_reason", unbanReason }
            };
            await ModLogAsync(context.Guild, keyValuePairs, DiscordEvent.Unban);

            await context.EditResponseAsync(new()
            {
                Content = $"{victim.Mention} has been unbanned{(sentDm ? "" : "(failed to dm)")}. Reason: {unbanReason}"
            });
        }
    }
}
