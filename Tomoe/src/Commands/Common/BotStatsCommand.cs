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
using EdgeDB;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class BotStatsCommand : BaseCommandModule
    {
        private static readonly Regex LastCommaRegex = new(", (?=[^,]*$)", RegexOptions.Compiled);
        private EdgeDBClient EdgeDBClient { get; init; }
        private CancellationTokenSource CancellationTokenSource { get; init; }

        public BotStatsCommand(EdgeDBClient edgeDBClient, CancellationTokenSource cancellationTokenSource)
        {
            EdgeDBClient = edgeDBClient;
            CancellationTokenSource = cancellationTokenSource;
        }

        [Command("bot_stats"), Description("Gets general info about the bot."), Aliases("bot_info", "bs", "bi", "bullshit")]
        public async Task BotStatsAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#6b73db")
            };
            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString("MB", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Process Memory", Process.GetCurrentProcess().WorkingSet64.Bytes().ToString("MB", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("CPU Thread Count", ThreadPool.ThreadCount.ToMetric(), true);
            embedBuilder.AddField("Bot Version", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            embedBuilder.AddField("Uptime", LastCommaRegex.Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);

            //CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationTokenSource.Token);
            //cancellationTokenSource.CancelAfter(5000);
            //embedBuilder.AddField("Total Guild Count", (await EdgeDBClient.QuerySingleAsync<long>("SELECT COUNT(SELECT Guild FILTER .disabled = false);", new Dictionary<string, object?>(), Capabilities.ReadOnly, cancellationTokenSource.Token)).ToString("N0", CultureInfo.InvariantCulture), true);
            //
            //cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationTokenSource.Token);
            //cancellationTokenSource.CancelAfter(5000);
            //embedBuilder.AddField("Total Member Count", (await EdgeDBClient.QuerySingleAsync<long>("SELECT COUNT(SELECT GuildMember FILTER .disabled = false);", new Dictionary<string, object?>(), Capabilities.ReadOnly, cancellationTokenSource.Token)).ToString("N0", CultureInfo.InvariantCulture), true);
            await context.RespondAsync(embedBuilder.Build());
        }
    }
}
