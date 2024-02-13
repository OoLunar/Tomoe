using System;
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
    public sealed class SleepCommand
    {
        [Command("mute"), TextAlias("sleep"), RequirePermissions(Permissions.ModerateMembers)]
        public static async ValueTask ExecuteAsync(CommandContext context, DiscordMember member, TimeSpan? timeSpan = null, [RemainingText] string? reason = null)
        {
            reason ??= "No reason provided.";
            timeSpan ??= TimeSpan.FromMinutes(5);
            await member.TimeoutAsync(DateTimeOffset.UtcNow.Add(timeSpan.Value), reason);
            await context.RespondAsync($"Muted {member.Mention} for {timeSpan}{(reason is null ? "." : $" for {reason}.")}");
        }
    }
}
