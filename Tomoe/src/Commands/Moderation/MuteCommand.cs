using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;

namespace Tomoe.Commands.Moderation
{
    public sealed class MuteCommand : ApplicationCommandModule
    {
        [SlashCommand("mute", "Prevents a user from having any sort of interaction in the guild."), Hierarchy(Permissions.ManageMessages)]
        public static async Task MuteAsync(InteractionContext context, [Option("person", "Who is being muted.")] DiscordMember member, [Option("length", "How long should they be muted.")] TimeSpan? length = null, [Option("reason", "Why are they being muted")] string reason = Constants.MissingReason)
        {
            if (length is not null && length <= TimeSpan.FromSeconds(5))
            {
                await context.EditResponseAsync(new()
                {
                    Content = "Error: The mute length must be at least 5 seconds."
                });
                return;
            }
            else if (member.CommunicationDisabledUntil is not null && member.CommunicationDisabledUntil > DateTimeOffset.UtcNow)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error {member.Mention} is already muted until {Formatter.Timestamp(member.CommunicationDisabledUntil.Value, TimestampFormat.ShortTime)}."
                });
                return;
            }

            length ??= TimeSpan.FromMinutes(5);

            bool sentDm = false;
            try
            {
                await member.SendMessageAsync($"You have been muted in {context.Guild.Name} until {Formatter.Timestamp(DateTimeOffset.UtcNow + length.Value, TimestampFormat.ShortTime)} for: {reason}.");
                sentDm = true;
            }
            catch (DiscordException) { }

            try
            {
                await member.TimeoutAsync(DateTimeOffset.UtcNow.Add(length.Value), reason);
            }
            catch (DiscordException error)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Discord Error {error.WebResponse.ResponseCode}, failed to mute {member.Mention} ({Formatter.InlineCode(member.Id.ToString(CultureInfo.InvariantCulture))}): {error.JsonMessage}"
                });

                if (sentDm)
                {
                    await member.SendMessageAsync($"Nevermind, I failed to mute you. Please contact {context.User.Mention} ({Formatter.InlineCode(context.User.Id.ToString(CultureInfo.InvariantCulture))}) for more information.");
                }
                return;
            }

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", context.Guild.Name },
                { "guild_count", Program.TotalMemberCount[context.Guild.Id].ToString("N0") },
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

            await ModLogCommand.ModLogAsync(context.Guild, keyValuePairs, DiscordEvent.Mute);
            await context.EditResponseAsync(new()
            {
                Content = $"{member.Mention} ({Formatter.InlineCode(member.Id.ToString(CultureInfo.InvariantCulture))}) is muted until {Formatter.Timestamp(length.Value, TimestampFormat.RelativeTime)}{(sentDm ? "" : " (failed to dm)")}.\nReason: {reason}"
            });
        }
    }
}
