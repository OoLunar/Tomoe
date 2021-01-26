using System.Diagnostics;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class BotInfo : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.BotInfo");

		[Command("botinfo"), Description("Gets general info about the bot."), Aliases("bot_info")]
		public async Task Get(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace("Creating embed...");
			DiscordEmbedBuilder embedBuilder = new();
			_logger.Trace("Setting title...");
			embedBuilder.Title = "General Bot Info";
			_logger.Trace("Filling out description...");
			embedBuilder.Description += $"Currently in {context.Client.Guilds.Count} guilds\n";
			embedBuilder.Description += $"Handling around {context.Client.Presences.Count} guild members\n";
			embedBuilder.Description += $"General Ping: {context.Client.Ping}ms\n";
			embedBuilder.Description += $"Total shards: {Program.Client.ShardClients.Count}\n";
			_logger.Trace("Getting resource usage...");
			Process currentProcess = Process.GetCurrentProcess();
			embedBuilder.Description += $"Total memory used: {currentProcess.PrivateMemorySize64 / 1024 / 1024}mb\n";
			embedBuilder.Description += $"Total threads open: {currentProcess.Threads.Count}";
			currentProcess.Dispose();
			_logger.Trace("Disposing of Process.GetCurrentProcess()...");
			_logger.Trace("Sending embed...");
			_ = Program.SendMessage(context, null, embedBuilder.Build());
			_logger.Trace("Embed sent!");
		}
	}
}
