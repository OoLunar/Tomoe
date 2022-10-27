namespace Tomoe.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
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

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", context.Guild.Name },
                { "guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric() },
                { "guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_username", victimMember.Username },
                { "victim_tag", victimMember.Discriminator },
                { "victim_mention", victimMember.Mention },
                { "victim_id", victimMember.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_displayname", victimMember.DisplayName },
                { "moderator_username", context.Member.Username },
                { "moderator_tag", context.Member.Discriminator },
                { "moderator_mention", context.Member.Mention },
                { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_displayname", context.Member.DisplayName },
                { "punishment_reason", reason }
            };
            await ModLog(context.Guild, keyValuePairs, DiscordEvent.Ban);

            await context.EditResponseAsync(new()
            {
                Content = $"{victimUser.Mention} has been banned{(sentDm ? "" : "(failed to dm)")}. Reason: {reason}"
            });
        }
    }
}
