using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Teehee I changed my mind.
    /// </summary>
    public static class UnbanCommand
    {
        /// <summary>
        /// Unbans a user from the server.
        /// </summary>
        /// <remarks>
        /// TODO: Add a unban requests system.
        /// </remarks>
        /// <param name="user">Who is being unbanned.</param>
        /// <param name="reason">Why they're being unbanned.</param>
        [Command("unban"), RequirePermissions(DiscordPermissions.BanMembers), RequireGuild]
        public static async ValueTask ExecuteAsync(CommandContext context, DiscordUser user, [RemainingText] string? reason = null)
        {
            // TODO: When the bot starts up, grab the most recent ban from the database
            // And use DiscordGuild.GetBansAsync(limit: 1000, before: null, after: mostRecentBan.UserId)
            // To populate the database with the most recent bans. Then we can use
            // GuildMemberModel.IsUserBannedAsync(user.Id, context.Guild!.Id) to check if the user is banned.
            try
            {
                await context.Guild!.GetBanAsync(user.Id);
            }
            catch (NotFoundException)
            {
                await context.RespondAsync("This user isn't banned from the server.");
                return;
            }

            // Defer since we're making multiple rest requests.
            await context.DeferResponseAsync();

            // Try/catch since member's can block DMs.
            bool didDm = false;
            if (!user.IsBot)
            {
                // Try to get the user
                foreach (ulong guildId in await GuildMemberModel.FindMutualGuildsAsync(user.Id))
                {
                    try
                    {
                        DiscordGuild guild = await context.Client.GetGuildAsync(guildId);
                        DiscordMember member = await guild.GetMemberAsync(user.Id);

                        DiscordEmbedBuilder embedBuilder = new();
                        embedBuilder.WithTitle($"You've been banned from {context.Guild!.Name}.");
                        embedBuilder.WithDescription(string.IsNullOrWhiteSpace(reason) ? "No reason was provided for the ban." : $"Reason: {reason}");
                        embedBuilder.AddField("Banned by", $"{context.User.Mention} (`{context.User.Id}`)");

                        await member.SendMessageAsync(embedBuilder);
                        didDm = true;
                        break;
                    }
                    catch (DiscordException) { }
                }
            }

            // Actually ban the user.
            await context.Guild!.UnbanMemberAsync(user.Id, reason ?? "No reason provided.");

            // Use a string builder since we don't want multiple inline ternaries.
            StringBuilder stringBuilder = new();
            stringBuilder.Append(CultureInfo.InvariantCulture, $"Unbanned {user.Mention}");
            if (!didDm)
            {
                stringBuilder.Append(" (DM failed)");
            }

            stringBuilder.Append(string.IsNullOrWhiteSpace(reason) ? "." : $" for: {reason}");
            await context.RespondAsync(stringBuilder.ToString());
        }
    }
}
