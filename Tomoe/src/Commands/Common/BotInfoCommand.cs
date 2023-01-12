using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Managers;
using DSharpPlus.CommandAll.Managers;
using DSharpPlus.CommandAll.Parsers;
using DSharpPlus.CommandAll.Parsers;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.Tomoe.Database;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class BotInfoCommand : BaseCommand
    {
        private static readonly ReadOnlyMemory<char> _slashPrefix = new[] { ',', ' ', '`', '/', '`' };
        private static readonly Regex _osNameRegex = GetOSNameRegex();
        private readonly DatabaseContext _databaseContext;

        public BotInfoCommand(DatabaseContext databaseContext) => _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));

        [Command("bot_info", "bs", "bot_stats")]
        public Task ExecuteAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#6b73db")
            };

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();
            _ = embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(true).Bytes().ToString(), true);
            _ = embedBuilder.AddField("Process Memory", currentProcess.WorkingSet64.Bytes().ToString(), true);
            _ = embedBuilder.AddField("Runtime Version", RuntimeInformation.FrameworkDescription, true);

            // Thank you @NaamloosDT/<@127408598010560513> for the code below
            string os;
            try
            {
                string result = File.ReadAllText("/etc/os-release");
                Match match = _osNameRegex.Match(result);
                os = !match.Success ? Environment.OSVersion.VersionString : match.Groups[1].Value.Replace("\"", "");
            }
            catch (Exception)
            {
                os = Environment.OSVersion.VersionString;
            }

            _ = embedBuilder.AddField("Operating System", $"{os} {RuntimeInformation.OSArchitecture.ToString().ToLower(CultureInfo.InvariantCulture)}", true);
            _ = embedBuilder.AddField("Uptime", LastCommaRegex().Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);
            _ = embedBuilder.AddField("Websocket Ping", LastCommaRegex().Replace(context.Client.Ping.Milliseconds().Humanize(3), " and "), true);

            _ = embedBuilder.AddField("Guild Count", _databaseContext.Guilds.LongCount().ToString("N0"), true);
            _ = embedBuilder.AddField("User Count", _databaseContext.Members.LongCount().ToString("N0"), true);

            /*
            List<string> prefixes = new();
            foreach (string prefix in ((PrefixParser)context.Extension.PrefixParser).Prefixes)
            {
                prefixes.Add($"`{prefix}`");
            }

            prefixes.Add("`/`");
            prefixes.Add(context.Client.CurrentUser.Mention);
            embedBuilder.AddField("Prefixes", string.Join(", ", prefixes), true);
            */

            // Thank you Aki for teaching me about Spans <3
            int i = 0;
            Span<char> prefixes = new char[1024]; // Max embed field length
            foreach (string prefix in ((PrefixParser)context.Extension.PrefixParser).Prefixes)
            {
                if (i != 0)
                {
                    prefixes[^2] = ',';
                    prefixes[^1] = ' ';
                }

                ReadOnlySpan<char> prefixSpan = prefix.AsSpan();
                prefixes[i++] = '`';
                prefixSpan.CopyTo(prefixes[i..(i += prefixSpan.Length)]);
                prefixes[i++] = '`';
            }

            _slashPrefix.Span.CopyTo(prefixes[i..(i += 5)]);
            ReadOnlySpan<char> mentionPrefix = context.Client.CurrentUser.Mention.AsSpan();
            prefixes[i++] = ',';
            prefixes[i++] = ' ';
            mentionPrefix.CopyTo(prefixes[i..(i += mentionPrefix.Length)]);
            _ = embedBuilder.AddField("Prefixes", prefixes.ToString().Trim('\0'), true);

            _ = embedBuilder.AddField("Bot Version", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            _ = embedBuilder.AddField("DSharpPlus Library Version", typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            _ = embedBuilder.AddField("CommandAll Library Version", typeof(CommandManager).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);


            return context.ReplyAsync(new DiscordMessageBuilder().AddEmbed(embedBuilder));
        }

        [GeneratedRegex(", (?=[^,]*$)", RegexOptions.Compiled)]
        private static partial Regex LastCommaRegex();
        [GeneratedRegex("PRETTY_NAME=(.*)", RegexOptions.Compiled)]
        private static partial Regex GetOSNameRegex();
    }
}
