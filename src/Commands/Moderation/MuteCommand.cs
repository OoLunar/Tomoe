using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Sometimes I wish I was non-verbal.
    /// </summary>
    public static class MuteCommand
    {
        /// <summary>
        /// Prevents a member from sending messages in chat.
        /// </summary>
        /// <param name="member">Who lost talking privileges?</param>
        /// <param name="timeSpan">How long are they muted for?</param>
        /// <param name="reason">Why are they being muted?</param>
        [Command("mute"), RequirePermissions(DiscordPermission.ModerateMembers)]
        public static async ValueTask MuteAsync(CommandContext context, DiscordMember? member = null, TimeSpan? timeSpan = null, [RemainingText] string? reason = null) => await ExecuteAsync(context, "Muted {0} for {1}. Reason: {2}", member, timeSpan, reason);

        /// <summary>
        /// Sometimes people stay up too late. This helps them get some rest.
        /// </summary>
        /// <param name="member">Who's gonna go to bed?</param>
        /// <param name="timeSpan">How long are they sleeping for?</param>
        [Command("sleep"), RequirePermissions(DiscordPermission.ModerateMembers)]
        public static async ValueTask SleepAsync(CommandContext context, DiscordMember? member = null, TimeSpan? timeSpan = null) => await ExecuteAsync(context, "Go sleep {0}. I'll see you in {1}.", member, timeSpan, null);

        private static async ValueTask ExecuteAsync(CommandContext context, string muteText, DiscordMember? member = null, TimeSpan? timeSpan = null, [RemainingText] string? reason = null)
        {
            if (member is null)
            {
                if (context is not TextCommandContext textCommandContext || textCommandContext.Message.ReferencedMessage is null)
                {
                    await context.RespondAsync("Who am I supposed to mute? Is this some cruel joke?");
                    return;
                }

                member = await context.Guild!.GetMemberAsync(textCommandContext.Message.ReferencedMessage.Author!.Id);
            }

            reason ??= "No reason provided.";
            timeSpan ??= TimeSpan.FromMinutes(5);
            await member.TimeoutAsync(DateTimeOffset.UtcNow.Add(timeSpan.Value), $"Requested by {context.Member!.GetDisplayName()} ({context.Member!.Id}): {reason}");
            await context.RespondAsync(string.Format(await context.GetCultureAsync(), muteText, member.Mention, timeSpan.Value.Humanize(1, await context.GetCultureAsync()), reason));
        }
    }
}
