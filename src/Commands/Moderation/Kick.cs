namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Tomoe.Api.Attributes;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommand("kick", "Kicks a member from the guild, sending them off with a dm."), Hierarchy]
        public static async Task Kick(InteractionContext context, [Option("victim", "Who to kick from the guild.")] DiscordUser victimUser, [Option("reason", "Why is the victim being kicked from the guild?")] string kickReason = Constants.MissingReason)
        {
            DiscordMember victimMember = await victimUser.Id.GetMember(context.Guild);
            if (victimMember == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    IsEphemeral = true,
                    Content = $"Error: {victimUser.Mention} is not in the guild!"
                });
                return;
            }
            else
            {
                if (victimMember.Hierarchy >= victimMember.Hierarchy)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        IsEphemeral = true,
                        Content = $"Error: {victimMember.Mention}'s highest role is equal or greater than your highest role! You don't have enough power over them."
                    });
                    return;
                }
                else if (victimMember.Hierarchy >= context.Guild.CurrentMember.Hierarchy)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        IsEphemeral = true,
                        Content = $"Error: {victimMember.Mention}'s highest role is equal or greater than my highest role! I don't have enough power over them."
                    });
                    return;
                }
            }

            await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new());

            bool sentDm = await victimUser.TryDmMember($"You've been kicked from {context.Guild.Name} by {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}). Reason: {kickReason}");
            await victimMember.RemoveAsync(kickReason);

            Dictionary<string, string> keyValuePairs = new();
            keyValuePairs.Add("guild_name", context.Guild.Name);
            keyValuePairs.Add("guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric());
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
            keyValuePairs.Add("punishment_reason", kickReason);
            await Api.Moderation.Modlog(context.Guild, keyValuePairs, Api.Moderation.LogType.Ban);

            await context.EditResponseAsync(new()
            {
                Content = $"{victimUser.Mention} has been kicked{(sentDm ? "" : "(failed to dm)")}. Reason: {kickReason}"
            });
        }
    }
}