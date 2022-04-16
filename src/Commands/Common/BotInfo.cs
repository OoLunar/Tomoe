using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace Tomoe.Commands.Common
{
    public class BotInfo : BaseCommandModule
    {
        [Command("bot_info"), Description("Gets general info about the bot."), Aliases("bot_stats")]
        public Task BotInfoAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#7b84d1")
            };
            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(true).Bytes().ToString("MB", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Process Memory", Process.GetCurrentProcess().WorkingSet64.Bytes().ToString("MB", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Thread Count", ThreadPool.ThreadCount.ToMetric(), true);
            embedBuilder.AddField("Uptime", (Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), true);
            embedBuilder.AddField("Guild Count", context.Client.Guilds.Count.ToMetric(), true);
            //embedBuilder.AddField("Member Count", DatabaseContext.GuildConfigs.Select(guildConfig => guildConfig.MemberCount).Sum().ToMetric(), true);

            return context.RespondAsync(embedBuilder.Build());
        }
    }
}
