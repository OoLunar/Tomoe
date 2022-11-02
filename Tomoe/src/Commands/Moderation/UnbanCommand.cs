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
    public sealed class UnbanCommand : ApplicationCommandModule
    {
        [SlashCommand("unban", "Unbans a person from the guild."), Hierarchy(Permissions.BanMembers)]
        public static async Task UnbanAsync(InteractionContext context, [Option("user_id", "The Discord User Id of whom to unban.")] string userIdString, [Option("reason", "Why is the user being unbanned from the guild.")] string unbanReason = Constants.MissingReason)
        {
            if (!ulong.TryParse(userIdString, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong userId))
            {
                // Inform the user that they didn't pass the user id and teach them how to get the user's id.
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {Formatter.InlineCode(userIdString)} isn't a Discord User Id. To get a user's id, go to your Discord Client's settings -> Advanced -> Developer Mode. Make sure to turn on the switch. Make the switch green. Make the white circle go from the left part of the switch to the right. After you have enabled Developer Mode, right click on either the user's profile or someone mentioning the user. Click \"Copy Id\". Then paste (Ctrl+V) the id into the {Formatter.InlineCode("/unban")} command."
                });
                return;
            }

            DiscordUser user = await context.Client.GetUserAsync(userId);
            if (user is null)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: <@{userId}> ({Formatter.InlineCode(userId.ToString(CultureInfo.InvariantCulture))}) is not a Discord User!"
                });
                return;
            }

            DiscordBan ban;
            try
            {
                ban = await context.Guild.GetBanAsync(user.Id);
            }
            catch (DiscordException error)
            {
                await context.EditResponseAsync(new()
                {
                    Content = error.WebResponse.ResponseCode == 404 ? $"Error: {user.Mention} is not banned!" : $"Discord Error {error.WebResponse.ResponseCode}, failed to grab ban information for {user.Mention} ({Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture))}):\n>>> {error.JsonMessage}"
                });
                return;
            }

            await context.Guild.UnbanMemberAsync(user.Id, unbanReason);

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", context.Guild.Name },
                { "guild_count", Program.TotalMemberCount[context.Guild.Id].ToMetric() },
                { "guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_username", user.Username },
                { "victim_tag", user.Discriminator },
                { "victim_mention", user.Mention },
                { "victim_id", user.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_username", context.Member.Username },
                { "moderator_tag", context.Member.Discriminator },
                { "moderator_mention", context.Member.Mention },
                { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_displayname", context.Member.DisplayName },
                { "punishment_reason", unbanReason }
            };

            await ModLogCommand.ModLogAsync(context.Guild, keyValuePairs, DiscordEvent.Unban);
            await context.EditResponseAsync(new()
            {
                Content = $"{user.Mention} has been unbanned.\nPrevious ban reason:\n> {ban.Reason}\nUnban Reason:\n>>> {unbanReason}"
            });
        }
    }
}
