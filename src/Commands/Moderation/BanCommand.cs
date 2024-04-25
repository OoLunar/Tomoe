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
    public static class BanCommand
    {
        [Command("ban"), RequirePermissions(DiscordPermissions.BanMembers), RequireGuild]
        public static async ValueTask ExecuteAsync(CommandContext context, DiscordUser user, [RemainingText] string? reason = null)
        {
            if (await GuildMemberModel.IsUserBannedAsync(user.Id, context.Guild!.Id))
            {
                await context.RespondAsync("This user is already banned.");
                return;
            }

            // Defer since we're making multiple rest requests.
            await context.DeferResponseAsync();

            // Try/catch since member's can block DMs.
            bool didDm = true;
            try
            {
                // Try to get the user
                DiscordMember member = await context.Guild!.GetMemberAsync(user.Id);

                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.WithTitle($"You've been banned from {context.Guild!.Name}.");
                embedBuilder.WithDescription(string.IsNullOrWhiteSpace(reason) ? "No reason was provided for the ban." : $"Reason: {reason}");
                embedBuilder.AddField("Banned by", $"{context.User.Mention} (`{context.User.Id}`)");

                await member.SendMessageAsync(embedBuilder);
            }
            catch (DiscordException)
            {
                didDm = false;
            }

            // Actually ban the user.
            await context.Guild!.BanMemberAsync(user.Id, 0, reason ?? "No reason provided.");

            // Use a string builder since we don't want multiple inline ternaries.
            StringBuilder stringBuilder = new();
            stringBuilder.Append(CultureInfo.InvariantCulture, $"Banned {user.Mention}");
            if (!didDm)
            {
                stringBuilder.Append(" (DM failed)");
            }

            stringBuilder.Append(string.IsNullOrWhiteSpace(reason) ? "." : $" for: {reason}");
            await context.RespondAsync(stringBuilder.ToString());
        }
    }
}
