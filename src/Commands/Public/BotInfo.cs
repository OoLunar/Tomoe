using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class BotInfo : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.BotInfo");

		[Command("botinfo"), Description("Gets general info about the bot."), Aliases("bot_info")]
		public async Task Overload(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace("Creating embed...");
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, "General Bot Info");
			StringBuilder botInfo = new();
			_logger.Trace("Filling out description...");
			_ = botInfo.Append($"Currently in {context.Client.Guilds.Count} guilds\n");
			_ = botInfo.Append($"Handling around {context.Client.Guilds.Values.SelectMany(g => g.Members.Keys).Count()} guild members\n");
			_ = botInfo.Append($"General Ping: {context.Client.Ping}ms\n");
			_ = botInfo.Append($"Total shards: {Program.Client.ShardClients.Count}\n");
			_logger.Trace("Getting resource usage...");
			Process currentProcess = Process.GetCurrentProcess();
			_ = botInfo.Append($"Total memory used: {currentProcess.PrivateMemorySize64 / 1024 / 1024}mb\n");
			_ = botInfo.Append($"Total threads open: {currentProcess.Threads.Count}");
			embedBuilder.Description = botInfo.ToString();
			currentProcess.Dispose();
			_logger.Trace("Disposing of Process.GetCurrentProcess()...");
			_logger.Trace("Sending embed...");
			_ = await Program.SendMessage(context, null, embedBuilder.Build());
			_logger.Trace("Embed sent!");
		}
	}
}
