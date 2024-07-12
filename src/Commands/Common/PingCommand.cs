using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Ping! Pong!
    /// </summary>
    public static class PingCommand
    {
        /// <summary>
        /// Sends the latency of the bot's connection to Discord.
        /// </summary>
        [Command("ping"), TextAlias("pong")]
        public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"Pong! Latency is {context.Client.GetConnectionLatency(context.Guild?.Id ?? 0).Humanize(3)}.");
    }
}
