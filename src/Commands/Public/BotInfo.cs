namespace Tomoe.Commands.Public
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Humanizer;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    public class BotInfo : BaseCommandModule
    {
        [Command("bot_info"), Description("Gets general info about the bot.")]
        public async Task Overload(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, "General Bot Info");
            StringBuilder botInfo = new();
            botInfo.Append($"Currently in {context.Client.Guilds.Count} guilds\n");
            int totalMemberCount = 0;
            foreach (int i in Listeners.GuildDownloadCompleted.MemberCount.Values)
            {
                totalMemberCount += i;
            }
            botInfo.Append($"Handling around {totalMemberCount.ToMetric()} guild members\n");
            botInfo.Append($"General Ping: {context.Client.Ping}ms\n");
            botInfo.Append($"Total shards: {Program.Client.ShardClients.Count}\n");
            Process currentProcess = Process.GetCurrentProcess();
            botInfo.Append($"Total memory used: {Math.Round(currentProcess.PrivateMemorySize64.Bytes().Megabytes, 2).ToMetric()}mb\n");
            botInfo.Append($"Total threads open: {currentProcess.Threads.Count}");
            embedBuilder.Description = botInfo.ToString();
            currentProcess.Dispose();
            await Program.SendMessage(context, null, embedBuilder.Build());
        }
    }
}
