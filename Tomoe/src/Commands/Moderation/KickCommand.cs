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
    public sealed class KickCommand : ApplicationCommandModule
    {
        [SlashCommand("kick", "Kicks a member from the guild, sending them off with a dm."), Hierarchy(Permissions.KickMembers)]
        public static async Task KickAsync(InteractionContext context, [Option("person", "Who to kick from the guild.")] DiscordUser user, [Option("reason", "Why is the victim being kicked from the guild?")] string reason = Constants.MissingReason)
        {
            // Test if the user is in the guild and attempt to DM them informing them they've been kicked.
            bool sentDm;
            DiscordMember member = null!; // shouldn't be null cause the try/catch will either set it or throw an exception and return
            try
            {
                member = await context.Guild.GetMemberAsync(user.Id);
                await member.SendMessageAsync($"You've been kicked from {context.Guild.Name} by {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}). Reason: {reason}");
                sentDm = true;
            }
            catch (DiscordException error) when (error.WebResponse.ResponseCode == 404)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {user.Mention} ({Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture))}) is not in this guild."
                });
                return;
            }

            // Try to kick them, if it fails then let the moderator know and attempt to let the user know we failed.
            try
            {
                await member.RemoveAsync(reason);
            }
            catch (DiscordException error)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Discord Error {error.WebResponse.ResponseCode}, failed to kick {user.Mention} ({Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture))}): {error.JsonMessage}"
                });

                // If we were able to DM them, let them know we failed.
                if (sentDm)
                {
                    await member.SendMessageAsync($"Nevermind! I failed to kick you from {context.Guild.Name}. Please reach out to {context.Member.Mention} ({Formatter.InlineCode(context.Member.Id.ToString(CultureInfo.InvariantCulture))}) for more information.");
                }
                return;
            }

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

            await ModLogCommand.ModLogAsync(context.Guild, keyValuePairs, DiscordEvent.Ban);
            await context.EditResponseAsync(new()
            {
                Content = $"{member.Mention} has been kicked{(sentDm ? "" : "(failed to dm)")}. Reason: {reason}"
            });
        }
    }
}
