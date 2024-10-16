using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Sometimes I wish I was verbal.
    /// </summary>
    public static class UnmuteCommand
    {
        /// <summary>
        /// Allows a member to send messages in chat again.
        /// </summary>
        /// <param name="member">Who gets to talk again?</param>
        /// <param name="reason">Why are they being unmuted?</param>
        [Command("unmute"), RequirePermissions(DiscordPermissions.ModerateMembers)]
        public static async ValueTask UnmuteAsync(CommandContext context, DiscordMember? member = null, [RemainingText] string? reason = null) => await ExecuteAsync(context, "Unmuted {0}. Reason: {1}", member, reason);

        /// <summary>
        /// Sometimes people sleep in for too long. This helps them wake up.
        /// </summary>
        /// <param name="member">Who's gonna wake up?</param>
        [Command("wake"), RequirePermissions(DiscordPermissions.ModerateMembers)]
        public static async ValueTask WakeAsync(CommandContext context, DiscordMember? member = null) => await ExecuteAsync(context, "# *WAKE UP {0}!*", member, null);

        private static async ValueTask ExecuteAsync(CommandContext context, string muteText, DiscordMember? member = null, [RemainingText] string? reason = null)
        {
            if (member is null)
            {
                if (context is not TextCommandContext textCommandContext || textCommandContext.Message.ReferencedMessage is null)
                {
                    await context.RespondAsync("Who am I supposed to unmute? Am I supposed to guess?");
                    return;
                }

                member = await context.Guild!.GetMemberAsync(textCommandContext.Message.ReferencedMessage.Author!.Id);
            }

            reason ??= "None provided.";
            await member.TimeoutAsync(null, reason);
            await context.RespondAsync(string.Format(await context.GetCultureAsync(), muteText, member.Mention, reason));
        }
    }
}
