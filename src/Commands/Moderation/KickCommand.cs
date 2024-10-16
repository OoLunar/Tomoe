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
    /// I kicked your shin.
    /// </summary>
    public static class KickCommand
    {
        /// <summary>
        /// Kicks a user from the server. Will NOT remove their messages. Attempts to DM the user with the reason for the kick.
        /// </summary>
        /// <param name="user">Who to kick.</param>
        /// <param name="reason">Why they're being removed.</param>
        [Command("kick"), RequirePermissions(DiscordPermissions.KickMembers)]
        public static async ValueTask ExecuteAsync(CommandContext context, DiscordUser user, [RemainingText] string? reason = null)
        {
            if (await GuildMemberModel.IsUserAbsentAsync(user.Id, context.Guild!.Id))
            {
                await context.RespondAsync("This user isn't in the guild.");
                return;
            }

            // Defer since we're making multiple rest requests.
            await context.DeferResponseAsync();

            // Try/catch since member's can block DMs.
            bool didDm = true;
            try
            {
                if (!user.IsBot)
                {
                    // Try to get the user
                    DiscordMember member = await context.Guild!.GetMemberAsync(user.Id);

                    DiscordEmbedBuilder embedBuilder = new();
                    embedBuilder.WithTitle($"You've been kicked from {context.Guild!.Name}.");
                    embedBuilder.WithDescription(string.IsNullOrWhiteSpace(reason) ? "No reason was provided for the kick." : $"Reason: {reason}");
                    embedBuilder.AddField("Kicked by", $"{context.User.Mention} (`{context.User.Id}`)");

                    await member.SendMessageAsync(embedBuilder);
                }
            }
            catch (DiscordException)
            {
                didDm = false;
            }

            // Actually kick the user.
            await context.Guild!.RemoveMemberAsync(user.Id, $"Requested by {context.Member!.GetDisplayName()} ({context.Member!.Id}): {reason ?? "No reason provided."}");

            // Use a string builder since we don't want multiple inline ternaries.
            StringBuilder stringBuilder = new();
            stringBuilder.Append(await context.GetCultureAsync(), $"Kicked {user.Mention}");
            if (!didDm)
            {
                stringBuilder.Append(" (DM failed)");
            }

            stringBuilder.Append(string.IsNullOrWhiteSpace(reason) ? "." : $" for: {reason}");
            await context.RespondAsync(stringBuilder.ToString());
        }
    }
}
