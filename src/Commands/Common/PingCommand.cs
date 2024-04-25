using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class PingCommand
    {
        [Command("ping"), TextAlias("pong"), RequirePermissions(DiscordPermissions.SendMessages | DiscordPermissions.SendMessagesInThreads | DiscordPermissions.AccessChannels, DiscordPermissions.None)]
        public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"Pong! Latency is {context.Client.Ping}ms.");
    }
}
