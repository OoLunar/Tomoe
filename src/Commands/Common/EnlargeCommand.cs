using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// I take pills to do this!
    /// </summary>
    public sealed class EnlargeCommand
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new <see cref="EnlargeCommand"/> with the specified <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use for fetching the emoji.</param>
        public EnlargeCommand(HttpClient httpClient) => _httpClient = httpClient;

        /// <summary>
        /// Fetches and sends a bigger version of the provided emoji.
        /// </summary>
        /// <param name="emoji">The emoji to enlarge.</param>
        [Command("enlarge")]
        public async ValueTask EnlargeAsync(CommandContext context, string emoji)
        {
            Uri? emojiUrl;
            if (DiscordEmoji.TryFromUnicode(context.Client, emoji, out DiscordEmoji? discordEmoji))
            {
                emojiUrl = new Uri($"https://raw.githubusercontent.com/twitter/twemoji/master/assets/72x72/{char.ConvertToUtf32(discordEmoji.Name, 0).ToString("X4", CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture)}.png");
            }
            else if (DiscordEmoji.TryFromName(context.Client, emoji, out discordEmoji))
            {
                UriBuilder uriBuilder = new(discordEmoji.Url)
                {
                    Query = "size=4048"
                };

                uriBuilder.Path = Path.ChangeExtension(uriBuilder.Uri.LocalPath, ".png");
                emojiUrl = uriBuilder.Uri;
            }
            else
            {
                Match match = InfoCommand.GetEmojiRegex().Match(emoji);
                if (!match.Success)
                {
                    await context.RespondAsync("Invalid emoji.");
                    return;
                }

                emojiUrl = new Uri($"https://cdn.discordapp.com/emojis/{match.Groups[2].Value}.png?size=4048");
            }

            using HttpResponseMessage response = await _httpClient.GetAsync(emojiUrl);
            if (!response.IsSuccessStatusCode)
            {
                await context.RespondAsync($"Failed to fetch the emoji: HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                return;
            }

            await context.RespondAsync(new DiscordMessageBuilder().WithContent($"-# <{emojiUrl}>").AddFile(Path.GetFileName(emojiUrl.LocalPath), await response.Content.ReadAsStreamAsync(), AddFileOptions.CloseStream));
        }
    }
}
