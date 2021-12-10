namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;

    public partial class Moderation : ApplicationCommandModule
    {
        [SlashCommand("ban", "Bans a member from the guild, sending them off with a dm."), Hierarchy(Permissions.BanMembers, true)]
        public static async Task Ban(InteractionContext context, [Option("victim", "Who to ban from the guild.")] DiscordUser victimUser, [Option("reason", "Why is the victim being banned from the guild?")] string reason = Constants.MissingReason)
        {
            try
            {
                await context.Guild.GetBanAsync(victimUser.Id);
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {victimUser.Mention} is already banned!"
                });
                return;
            }
            catch (Exception) { }

            DiscordMember victimMember = await victimUser.Id.GetMember(context.Guild);
            bool sentDm = await victimUser.TryDmMember($"You've been banned from {context.Guild.Name} by {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}). Reason: {reason}");
            await context.Guild.BanMemberAsync(victimUser.Id, 1, reason);

            Dictionary<string, string> keyValuePairs = new();
            keyValuePairs.Add("guild_name", context.Guild.Name);
            keyValuePairs.Add("guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric());
            keyValuePairs.Add("guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("victim_username", victimMember.Username);
            keyValuePairs.Add("victim_tag", victimMember.Discriminator);
            keyValuePairs.Add("victim_mention", victimMember.Mention);
            keyValuePairs.Add("victim_id", victimMember.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("victim_displayname", victimMember.DisplayName);
            keyValuePairs.Add("moderator_username", context.Member.Username);
            keyValuePairs.Add("moderator_tag", context.Member.Discriminator);
            keyValuePairs.Add("moderator_mention", context.Member.Mention);
            keyValuePairs.Add("moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("moderator_displayname", context.Member.DisplayName);
            keyValuePairs.Add("punishment_reason", reason);
            await ModLog(context.Guild, keyValuePairs, DiscordEvent.Ban);

            await context.EditResponseAsync(new()
            {
                Content = $"{victimUser.Mention} has been banned{(sentDm ? "" : "(failed to dm)")}. Reason: {reason}"
            });
        }
    }
}