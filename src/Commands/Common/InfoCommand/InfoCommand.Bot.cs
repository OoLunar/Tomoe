using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class InfoCommand
    {
        private static readonly string _operatingSystem = $"{Environment.OSVersion} {RuntimeInformation.OSArchitecture.ToString().ToLower(CultureInfo.InvariantCulture)}";
        private static readonly string _botVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        private static readonly string _dSharpPlusVersion = typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        [GeneratedRegex(", (?=[^,]*$)", RegexOptions.Compiled)]
        private static partial Regex _getLastCommaRegex();

        private readonly AllocationRateTracker _allocationRateTracker = new();

        /// <summary>
        /// Sends bot statistics.
        /// </summary>
        [Command("bot"), DefaultGroupCommand]
        public async ValueTask BotInfoAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#6b73db")
            };

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();

            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Process Memory", currentProcess.WorkingSet64.Bytes().ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Allocation Rate", $"{_allocationRateTracker.AllocationRate.Bytes().ToString(CultureInfo.InvariantCulture)}/s", true);

            embedBuilder.AddField("Runtime Version", RuntimeInformation.FrameworkDescription, true);
            embedBuilder.AddField("Operating System", _operatingSystem, true);
            embedBuilder.AddField("Uptime", _getLastCommaRegex().Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);

            embedBuilder.AddField("Discord Latency", _getLastCommaRegex().Replace(context.Client.Ping.Milliseconds().Humanize(3), " and "), true);
            embedBuilder.AddField("Guild Count", (await GuildMemberModel.CountGuildsAsync()).ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("User Count", (await GuildMemberModel.CountMembersAsync()).ToString("N0", CultureInfo.InvariantCulture), true);

            StringBuilder stringBuilder = new();
            stringBuilder.Append(context.Client.CurrentUser.Mention);
            stringBuilder.Append(", ");
            foreach (string prefix in ((DefaultPrefixResolver)context.Extension.GetProcessor<TextCommandProcessor>().Configuration.PrefixResolver.Target!).Prefixes)
            {
                stringBuilder.Append('`');
                stringBuilder.Append(prefix);
                stringBuilder.Append('`');
                stringBuilder.Append(", ");
            }

            stringBuilder.Append(", `/`");
            embedBuilder.AddField("Prefixes", stringBuilder.ToString(), true);
            embedBuilder.AddField("Bot Version", _botVersion, true);
            embedBuilder.AddField("DSharpPlus Library Version", _dSharpPlusVersion, true);

            await context.RespondAsync(embedBuilder);
        }
    }
}
