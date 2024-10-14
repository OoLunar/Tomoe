using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class InfoCommand
    {
        [GeneratedRegex("<a?:(\\w+):(\\d+)>", RegexOptions.Compiled)]
        private static partial Regex _getEmojiRegex();

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_UnicodeEmojis")]
        private static extern Dictionary<string, string> _unicodeEmojis(DiscordEmoji emoji);

        /// <summary>
        /// Sends information about the provided emoji.
        /// </summary>
        /// <param name="emoji">The emoji to get information about. Unicode emojis are supported.</param>
        [Command("emoji")]
        public async Task EmojiInfoAsync(CommandContext context, string emoji)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#6b73db")
            };

            // We parse the emoji by hand in case if the bot doesn't have access to the emoji.
            if (DiscordEmoji.TryFromUnicode(context.Client, emoji, out DiscordEmoji? discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", _unicodeEmojis(null!).First(x => x.Value == discordEmoji.Name).Key.Replace(":", "\\:"), true);
                embedBuilder.AddField("Unicode", $"\\{discordEmoji.Name}", true);
                embedBuilder.ImageUrl = $"https://raw.githubusercontent.com/twitter/twemoji/master/assets/72x72/{char.ConvertToUtf32(discordEmoji.Name, 0).ToString("X4", CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture)}.png";
            }
            else if (DiscordEmoji.TryFromName(context.Client, emoji, out discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", discordEmoji.Name, true);
                embedBuilder.AddField("Emoji ID", $"`{discordEmoji.Id.ToString(CultureInfo.InvariantCulture)}`", true);
                embedBuilder.ImageUrl = discordEmoji.Url;
            }
            else
            {
                Match match = _getEmojiRegex().Match(emoji);
                if (!match.Success)
                {
                    await context.RespondAsync("Invalid emoji.");
                    return;
                }

                embedBuilder.AddField("Emoji Name", match.Groups[1].Value, true);
                embedBuilder.AddField("Emoji ID", $"`{match.Groups[2].Value}`", true);
                embedBuilder.ImageUrl = $"https://cdn.discordapp.com/emojis/{match.Groups[2].Value}.png";
            }

            // ZWS field
            embedBuilder.AddField("\u200B", "\u200B", true);
            embedBuilder.AddField("Emoji URL", Formatter.MaskedUrl("Link to the image.", new Uri(embedBuilder.ImageUrl)), true);
            if (emoji.StartsWith("<a:", StringComparison.Ordinal))
            {
                embedBuilder.AddField("GIF URL", Formatter.MaskedUrl("Link to the GIF.", new Uri(embedBuilder.ImageUrl)), true);
                embedBuilder.ImageUrl = embedBuilder.ImageUrl.Replace(".png", ".gif");
            }

            ImageData? image = await _imageUtilitiesService.GetImageDataAsync(embedBuilder.ImageUrl);
            if (image is null)
            {
                embedBuilder.WithFooter("Failed to get image data.");
                await context.RespondAsync(embedBuilder);
                return;
            }

            if (image.FrameCount != 1)
            {
                // ZWS field
                embedBuilder.AddField("\u200B", "\u200B", true);
                embedBuilder.AddField("Format", image.Format, true);
                embedBuilder.AddField("Frame Count", image.FrameCount.ToString(await context.GetCultureAsync()), true);
                // ZWS field
                embedBuilder.AddField("\u200B", "\u200B", true);
            }
            else
            {
                embedBuilder.AddField("Format", image.Format, false);
            }

            embedBuilder.AddField("Resolution", image.Resolution, true);
            embedBuilder.AddField("Dimensions", image.Dimensions, true);
            embedBuilder.AddField("File Size", image.FileSize, true);

            await context.RespondAsync(embedBuilder);
        }
    }
}
