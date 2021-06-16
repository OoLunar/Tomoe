namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommand("unban", "Unbans a person from the guild.")]
        public static async Task Unban(InteractionContext context, [Option("victim_id", "The Discord user id of who to unban from the guild.")] ulong victimId, [Option("reason", "Why is the victim being unbanned from the guild.")] string unbanReason = Constants.MissingReason)
        {
            DiscordUser victim = await context.Client.GetUserAsync(victimId);
            if (victim == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    IsEphemeral = true,
                    Content = $"Error: <@{victimId}> ({victimId}) is not a Discord user!"
                });
                return;
            }

            if (!(await context.Guild.GetBansAsync()).Any(discordBan => discordBan.User.Id == victimId))
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    IsEphemeral = true,
                    Content = $"Error: <@{victimId}> is not banned!"
                });
                return;
            }

            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new());


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
            await Api.Moderation.Modlog(context.Guild, keyValuePairs, Api.Moderation.LogType.Ban);

            await context.EditResponseAsync(new()
            {
                Content = $"{victim.Mention} has been unbanned{(sentDm ? "" : "(failed to dm)")}. Reason: {unbanReason}"
            });
        }
    }
}