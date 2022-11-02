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
    public sealed class BanCommand : ApplicationCommandModule
    {
        [SlashCommand("ban", "Bans a member from the guild, sending them off with a dm."), Hierarchy(Permissions.BanMembers, true)]
        public static async Task BanAsync(InteractionContext context, [Option("person", "Who to ban from the guild.")] DiscordUser user, [Option("reason", "Why is the victim being banned from the guild?")] string reason = Constants.MissingReason, [Option("delete_message_days", "Remove their messages from the past X days. Default = 0")] int deleteMessageDays = 0)
        {
            // Test if they're banned already
            try
            {
                DiscordBan ban = await context.Guild.GetBanAsync(user.Id);
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {user.Mention} ({Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture))}) was already banned! Reason:\n>>> {ban.Reason}"
                });
                return;
            }
            catch (DiscordException error)
            {
                // Wanted to use a when clause but that breaks my intellisense/autocomplete.
                if (error.WebResponse.ResponseCode != 404)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Discord Error {error.WebResponse.ResponseCode}, failed to grab ban information for {user.Mention} ({Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture))}):\n>>> {error.JsonMessage}"
                    });
                    return;
                }
            }

            // Send a DM letting them know they've been banned
            bool sentDm = false;
            DiscordMember? member = null;
            try
            {
                member = await context.Guild.GetMemberAsync(user.Id);
                await member.SendMessageAsync($"You've been banned from {context.Guild.Name} by {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}). Reason:\n>>> {reason}");
                sentDm = true;
            }
            catch (DiscordException) { }

            // Try to ban them, if it fails then let the moderator know and attempt to let the user know we failed.
            try
            {
                await context.Guild.BanMemberAsync(user.Id, deleteMessageDays, reason);
            }
            catch (DiscordException error)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Discord Error {error.WebResponse.ResponseCode}, failed to ban {user.Mention} ({Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture))}):\n>>> {error.JsonMessage}"
                });

                // Try to let them know that we failed to ban them if we previously let them know that we were going to ban them.
                if (member is not null && sentDm)
                {
                    await member.SendMessageAsync($"Nevermind, I failed to ban you. Please contact {context.User.Mention} ({Formatter.InlineCode(context.User.Id.ToString(CultureInfo.InvariantCulture))}) for more information.");
                }
                return;
            }

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", context.Guild.Name },
                { "guild_count", Program.TotalMemberCount[context.Guild.Id].ToMetric() },
                { "guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_username", user.Username },
                { "victim_tag", user.Discriminator },
                { "victim_mention", user.Mention },
                { "victim_id", user.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_displayname", member?.DisplayName ?? user.Username },
                { "moderator_username", context.Member.Username },
                { "moderator_tag", context.Member.Discriminator },
                { "moderator_mention", context.Member.Mention },
                { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_displayname", context.Member.DisplayName },
                { "punishment_reason", reason }
            };

            await ModLogCommand.ModLogAsync(context.Guild, keyValuePairs, DiscordEvent.Ban);
            await context.EditResponseAsync(new()
            {
                Content = $"{user.Mention} has been banned{(sentDm ? "" : " (failed to dm)")}. Reason:\n>>> {reason}"
            });
        }
    }
}
