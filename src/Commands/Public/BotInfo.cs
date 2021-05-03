using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace Tomoe.Commands.Public
{
	public class BotInfo : BaseCommandModule
	{
		[Command("botinfo"), Description("Gets general info about the bot."), Aliases("bot_info")]
		public async Task Overload(CommandContext context)
		{
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, "General Bot Info");
			StringBuilder botInfo = new();
			_ = botInfo.Append($"Currently in {context.Client.Guilds.Count} guilds\n");
			_ = botInfo.Append($"Handling around {Listeners.GuildDownloadCompleted.MemberCount} guild members\n");
			_ = botInfo.Append($"General Ping: {context.Client.Ping}ms\n");
			_ = botInfo.Append($"Total shards: {Program.Client.ShardClients.Count}\n");
			Process currentProcess = Process.GetCurrentProcess();
			_ = botInfo.Append($"Total memory used: {Math.Round(currentProcess.PrivateMemorySize64.Bytes().Megabytes, 2)}mb\n");
			_ = botInfo.Append($"Total threads open: {currentProcess.Threads.Count}");
			embedBuilder.Description = botInfo.ToString();
			currentProcess.Dispose();
			_ = await Program.SendMessage(context, null, embedBuilder.Build());
		}
	}
}
