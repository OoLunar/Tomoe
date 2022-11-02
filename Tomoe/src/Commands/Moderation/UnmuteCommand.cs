using System;
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
    public sealed class UnmuteCommand : ApplicationCommandModule
    {
        [SlashCommand("unmute", "Allows the user to talk in the guild again."), Hierarchy(Permissions.ModerateMembers)]
        public static async Task UnmuteAsync(InteractionContext context, [Option("user", "Who is being unmuted?")] DiscordMember member, [Option("reason", "Why is the user being unmuted?")] string reason = Constants.MissingReason)
        {
            if (member.CommunicationDisabledUntil is null || member.CommunicationDisabledUntil < DateTimeOffset.UtcNow)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {member.Mention} isn't muted!"
                });
                return;
            }

            try
            {
                await member.TimeoutAsync(null, reason);
            }
            catch (DiscordException error)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Discord Error {error.WebResponse.ResponseCode}, failed to unmute {member.Mention} ({Formatter.InlineCode(member.Id.ToString(CultureInfo.InvariantCulture))}):\n>>> {error.JsonMessage}"
                });
                return;
            }

            bool sentDm = false;
            try
            {
                await member.SendMessageAsync($"You were unmuted by {context.User.Mention} ({Formatter.InlineCode(context.User.Id.ToString(CultureInfo.InvariantCulture))}) in {context.Guild.Name}. Reason:\n>>> {reason}");
                sentDm = true;
            }
            catch (DiscordException) { }

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", context.Guild.Name },
                { "guild_count", Program.TotalMemberCount[context.Guild.Id].ToMetric() },
                { "guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_username", member.Username },
                { "victim_tag", member.Discriminator },
                { "victim_mention", member.Mention },
                { "victim_id", member.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_displayname", member.DisplayName },
                { "moderator_username", context.Member.Username },
                { "moderator_tag", context.Member.Discriminator },
                { "moderator_mention", context.Member.Mention },
                { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_displayname", context.Member.DisplayName },
                { "punishment_reason", reason }
            };

            await ModLogCommand.ModLogAsync(context.Guild, keyValuePairs, CustomEvent.Unmute);
            await context.EditResponseAsync(new()
            {
                Content = $"{member.Mention} ({member.Username}#{member.Discriminator}) has been unmuted{(sentDm ? "" : " (failed to dm)")}.\nReason: {reason}"
            });
        }
    }
}
