using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("bot_info", "Gets general info about the bot.")]
        public static Task BotInfoAsync(InteractionContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#7b84d1")
            };

            embedBuilder.AddField("Guild Count", context.Client.Guilds.Count.ToMetric());
            embedBuilder.AddField("Member Count", TotalMemberCount.Values.Sum().ToMetric());
            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(true).Bytes().ToString("MB", CultureInfo.InvariantCulture));
            embedBuilder.AddField("Thread Count", ThreadPool.ThreadCount.ToMetric());
            embedBuilder.AddField("Websocket Ping", context.Client.Ping + "ms");
            embedBuilder.AddField("Uptime", (Process.GetCurrentProcess().StartTime.ToUniversalTime() - DateTime.UtcNow).Humanize(3));
            embedBuilder.AddField("Shard Count", Program.Client.ShardClients.Count.ToMetric());
            embedBuilder.AddField("Version", Constants.Version);

            return context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
        }
    }
}
