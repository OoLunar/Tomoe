using System.Diagnostics;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Utils;

namespace Tomoe.Commands.Public {
    public class BotInfo : BaseCommandModule {
        private static readonly Logger _logger = new Logger("Commands.Public.BotInfo");

        [Command("botinfo"), Description("Gets general info about the bot."), Aliases("bot_info")]
        public async Task Get(CommandContext context) {
            _logger.Trace("Creating embed...");
            DiscordEmbedBuilder embedBuilder = new();
            embedBuilder.Title = "General Bot Info";
            embedBuilder.Description += $"Currently in {context.Client.Guilds.Count} guilds\n";
            embedBuilder.Description += $"Handling around {context.Client.Presences.Count} people\n";
            embedBuilder.Description += $"General Ping: {context.Client.Ping}ms\n";
            embedBuilder.Description += $"Total shards: {Program.Client.ShardClients.Count}\n";
            Process currentProcess = Process.GetCurrentProcess();
            embedBuilder.Description += $"Total memory used: {currentProcess.PrivateMemorySize64 / 1024 / 1024}mb\n";
            embedBuilder.Description += $"Total threads open: {currentProcess.Threads.Count}";
            currentProcess.Dispose();
            Program.SendMessage(context, embedBuilder.Build());
        }
    }
}