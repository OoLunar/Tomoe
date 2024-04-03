using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public static class BanCommand
    {
        [Command("ban"), RequirePermissions(Permissions.BanMembers), RequireGuild]
        public static async ValueTask ExecuteAsync(CommandContext context, DiscordUser user, [RemainingText] string? reason = null)
        {
            await context.DeferResponseAsync();
            await context.Guild!.BanMemberAsync(user.Id, 0, reason ?? "No reason provided.");
            await context.RespondAsync($"Banned {user.Mention}{(reason is null ? "." : $" for: {reason}.")}");
        }
    }
}
