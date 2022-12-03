using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Managers;
using OoLunar.DSharpPlus.CommandAll.Parsers;
using OoLunar.Tomoe.Database;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class BotInfoCommand : BaseCommand
    {
        private static readonly ReadOnlyMemory<char> _slashPrefix = new[] { ',', ' ', '`', '/', '`' };
        private readonly DatabaseContext _databaseContext;

        public BotInfoCommand(DatabaseContext databaseContext) => _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));

        [Command("bot_info")]
        public Task ExecuteAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#6b73db")
            };

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();
            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString(), true);
            embedBuilder.AddField("Process Memory", currentProcess.WorkingSet64.Bytes().ToString(), true);
            embedBuilder.AddField("Runtime Version", RuntimeInformation.FrameworkDescription, true);

            embedBuilder.AddField("Operating System", $"{Environment.OSVersion} {RuntimeInformation.OSArchitecture.ToString().ToLower(CultureInfo.InvariantCulture)}", true);
            embedBuilder.AddField("Uptime", LastCommaRegex().Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);
            embedBuilder.AddField("Websocket Ping", LastCommaRegex().Replace(context.Client.Ping.Milliseconds().Humanize(3), " and "), true);

            embedBuilder.AddField("Guild Count", _databaseContext.Guilds.LongCount().ToString("N0"), true);
            embedBuilder.AddField("User Count", _databaseContext.Members.LongCount().ToString("N0"), true);

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
            embedBuilder.AddField("Prefixes", prefixes.ToString().Trim('\0'), true);

            embedBuilder.AddField("Bot Version", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            embedBuilder.AddField("DSharpPlus Library Version", typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            embedBuilder.AddField("CommandAll Library Version", typeof(CommandManager).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);


            return context.ReplyAsync(new DiscordMessageBuilder().AddEmbed(embedBuilder));
        }

        [GeneratedRegex(", (?=[^,]*$)", RegexOptions.Compiled)]
        private static partial Regex LastCommaRegex();
    }
}
