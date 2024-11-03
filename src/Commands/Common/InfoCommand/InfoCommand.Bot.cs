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
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class InfoCommand
    {
        [GeneratedRegex(", (?=[^,]*$)", RegexOptions.Compiled)]
        private static partial Regex _getLastCommaRegex();

        private static readonly string _operatingSystem = $"{Environment.OSVersion} {RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()}";
        private static readonly string _botVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        private static readonly string _dSharpPlusVersion = typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

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

            CultureInfo usersCulture = await context.GetCultureAsync();
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();

            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString(usersCulture), true);
            embedBuilder.AddField("Process Memory", currentProcess.WorkingSet64.Bytes().ToString(usersCulture), true);
            embedBuilder.AddField("Allocation Rate", $"{_allocationRateTracker.AllocationRate.Bytes().ToString(usersCulture)}/s", true);

            embedBuilder.AddField("Runtime Version", RuntimeInformation.FrameworkDescription, true);
            embedBuilder.AddField("Operating System", _operatingSystem, true);
            embedBuilder.AddField("Uptime", _getLastCommaRegex().Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3, usersCulture), " and "), true);

            embedBuilder.AddField("Discord Latency", _getLastCommaRegex().Replace(context.Client.GetConnectionLatency(context.Guild?.Id ?? 0).Humanize(3, usersCulture), " and "), true);
            embedBuilder.AddField("Guild Count", (await GuildMemberModel.CountGuildsAsync()).ToString("N0", usersCulture), true);
            embedBuilder.AddField("User Count", (await GuildMemberModel.CountMembersAsync()).ToString("N0", usersCulture), true);

            StringBuilder stringBuilder = new();
            stringBuilder.Append(context.Client.CurrentUser.Mention);
            stringBuilder.Append(", `");
            stringBuilder.Append(await GuildSettingsModel.GetTextPrefixAsync(context.Guild?.Id ?? 0) ?? _configuration.Discord.Prefix);
            stringBuilder.Append("`, `/`");

            embedBuilder.AddField("Prefixes", stringBuilder.ToString(), true);
            embedBuilder.AddField("Bot Version", _botVersion, true);
            embedBuilder.AddField("DSharpPlus Library Version", _dSharpPlusVersion, true);

            await context.RespondAsync(embedBuilder);
        }
    }
}
