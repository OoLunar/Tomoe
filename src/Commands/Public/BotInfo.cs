namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class Public : SlashCommandModule
    {
        [SlashCommand("bot_info", "Gets general info about the bot.")]
        public static async Task BotInfo(InteractionContext context)
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

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
        }
    }
}
