using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Ping! Pong!
    /// </summary>
    public static class WhoAmI
    {
        /// <summary>
        /// Sends the latency of the bot's connection to Discord.
        /// </summary>
        [Command("whoami"), TextAlias("who")]
        public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"I think you're {context.User.Mention}. I could be wrong though.");
    }
}
