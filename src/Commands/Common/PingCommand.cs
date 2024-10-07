using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Ping! Pong!
    /// </summary>
    public static class PingCommand
    {
        private class PingTranslator : IInteractionLocalizer
        {
            public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
            {
                "ping.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
                {
                    { DiscordLocale.en_US, "ping" },
                    { DiscordLocale.ja, "ピン" },
                    { DiscordLocale.tr, "pıng" }
                }),
                "ping.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
                {
                    { DiscordLocale.en_US, "Pings the bot to check its latency." },
                    { DiscordLocale.ja, "ボットにピンを送信して、その遅延を確認します。" },
                    { DiscordLocale.tr, "Botu pinglemek ve gecikmesini kontrol etmek için." }
                }),
                _ => throw new KeyNotFoundException()
            };
        }

        /// <summary>
        /// Sends the latency of the bot's connection to Discord.
        /// </summary>
        [Command("ping"), TextAlias("pong"), InteractionLocalizer<PingTranslator>()]
        public static async ValueTask ExecuteAsync(CommandContext context) => await context.RespondAsync($"Pong! Latency is {context.Client.GetConnectionLatency(context.Guild?.Id ?? 0).Humanize(3, await context.GetCultureAsync())}.");
    }
}
