using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class BotStats : BaseCommandModule
    {
        private static readonly Regex LastCommaRegex = new(", (?=[^,]*$)", RegexOptions.Compiled);

        [Command("bot_stats"), Description("Gets general info about the bot."), Aliases("bot_info", "bs", "bi", "bullshit")]
        public Task BotStatsAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#6b73db")
            };
            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString("MB", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Process Memory", Process.GetCurrentProcess().WorkingSet64.Bytes().ToString("MB", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Thread Count", ThreadPool.ThreadCount.ToMetric(), true);
            embedBuilder.AddField("Bot Version", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            embedBuilder.AddField("\u200b", "\u200b", true); // Blank field
            embedBuilder.AddField("Uptime", LastCommaRegex.Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);
            return context.RespondAsync(embedBuilder.Build());
        }
    }
}
