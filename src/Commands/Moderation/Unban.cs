namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommand("unban", "Unbans a person from the guild."), Hierarchy(Permissions.BanMembers)]
        public static async Task Unban(InteractionContext context, [Option("victim_id", "The Discord user id of who to unban from the guild.")] string victimIdString, [Option("reason", "Why is the victim being unbanned from the guild.")] string unbanReason = Constants.MissingReason)
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
            bool sentDm = await victim.TryDmMember($"You've been unbanned from {context.Guild.Name} by {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}). Reason: {unbanReason}");

            Dictionary<string, string> keyValuePairs = new();
            keyValuePairs.Add("guild_name", context.Guild.Name);
            keyValuePairs.Add("guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric());
            keyValuePairs.Add("person_username", victim.Username);
            keyValuePairs.Add("person_tag", victim.Discriminator);
            keyValuePairs.Add("person_mention", victim.Mention);
            keyValuePairs.Add("person_id", victim.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("moderator_username", context.Member.Username);
            keyValuePairs.Add("moderator_tag", context.Member.Discriminator);
            keyValuePairs.Add("moderator_mention", context.Member.Mention);
            keyValuePairs.Add("moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("moderator_displayname", context.Member.DisplayName);
            keyValuePairs.Add("punishment_reason", unbanReason);
            await ModLog(context.Guild, keyValuePairs, DiscordEvent.Ban);

            await context.EditResponseAsync(new()
            {
                Content = $"{victim.Mention} has been unbanned{(sentDm ? "" : "(failed to dm)")}. Reason: {unbanReason}"
            });
        }
    }
}