namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommand("kick", "Kicks a member from the guild, sending them off with a dm."), Hierarchy(Permissions.KickMembers)]
        public static async Task Kick(InteractionContext context, [Option("victim", "Who to kick from the guild.")] DiscordUser victimUser, [Option("reason", "Why is the victim being kicked from the guild?")] string reason = Constants.MissingReason)
        {
            DiscordMember victimMember = await victimUser.Id.GetMember(context.Guild);
            if (victimMember == null)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {victimUser.Mention} is not in the guild!"
                });
                return;
            }

            bool sentDm = await victimUser.TryDmMember($"You've been kicked from {context.Guild.Name} by {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}). Reason: {reason}");
            await victimMember.RemoveAsync(reason);

            Dictionary<string, string> keyValuePairs = new();
            keyValuePairs.Add("guild_name", context.Guild.Name);
            keyValuePairs.Add("guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric());
            keyValuePairs.Add("guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("person_username", victimMember.Username);
            keyValuePairs.Add("person_tag", victimMember.Discriminator);
            keyValuePairs.Add("person_mention", victimMember.Mention);
            keyValuePairs.Add("person_id", victimMember.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("person_displayname", victimMember.DisplayName);
            keyValuePairs.Add("moderator_username", context.Member.Username);
            keyValuePairs.Add("moderator_tag", context.Member.Discriminator);
            keyValuePairs.Add("moderator_mention", context.Member.Mention);
            keyValuePairs.Add("moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("moderator_displayname", context.Member.DisplayName);
            keyValuePairs.Add("punishment_reason", reason);
            await ModLog(context.Guild, keyValuePairs, DiscordEvent.Ban);

            await context.EditResponseAsync(new()
            {
                Content = $"{victimUser.Mention} has been kicked{(sentDm ? "" : "(failed to dm)")}. Reason: {reason}"
            });
        }
    }
}