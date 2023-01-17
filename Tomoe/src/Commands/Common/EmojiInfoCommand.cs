using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class EmojiInfoCommand : BaseCommand
    {
        private static readonly Regex EmojiRegex = GetEmojiRegex();
        private static readonly Dictionary<string, string> UnicodeEmojis = (Dictionary<string, string>)typeof(DiscordEmoji).GetProperty("UnicodeEmojis", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        private readonly ImageUtilitiesService _imageUtilitiesService;

        public EmojiInfoCommand(ImageUtilitiesService imageUtilitiesService) => _imageUtilitiesService = imageUtilitiesService;

        [Command("emoji_info")]
        public async Task ExecuteAsync(CommandContext context, string emoji)
        {
            // Grab the emoji name, id, filesize and url.
            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#6b73db")
            };

            if (DiscordEmoji.TryFromUnicode(context.Client, emoji, out DiscordEmoji? discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", UnicodeEmojis.First(x => x.Value == discordEmoji.Name).Key.Replace(":", "\\:"), true);
                embedBuilder.AddField("Unicode", $"\\{discordEmoji.Name}", true);
                embedBuilder.ImageUrl = $"https://raw.githubusercontent.com/twitter/twemoji/master/assets/72x72/{char.ConvertToUtf32(discordEmoji.Name, 0).ToString("X4").ToLower(CultureInfo.InvariantCulture)}.png";
            }
            else if (DiscordEmoji.TryFromName(context.Client, emoji, out discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", discordEmoji.Name, true);
                embedBuilder.AddField("Emoji ID", $"`{discordEmoji.Id.ToString(CultureInfo.InvariantCulture)}`", true);
                embedBuilder.ImageUrl = discordEmoji.Url;
            }
            else
            {
                Match match = EmojiRegex.Match(emoji);
                if (!match.Success)
                {
                    await context.ReplyAsync("Invalid emoji.");
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

            ImageData image = await _imageUtilitiesService.GetImageDataAsync(embedBuilder.ImageUrl);
            embedBuilder.AddField("Format", image.Format, false);
            embedBuilder.AddField("Resolution", image.Resolution, true);
            embedBuilder.AddField("Dimensions", image.Dimensions, true);
            embedBuilder.AddField("File Size", image.FileSize, true);

            if (image.FrameCount != 1)
            {
                embedBuilder.AddField("Frame Count", image.FrameCount.ToString(CultureInfo.InvariantCulture), true);
            }

            await context.ReplyAsync(embedBuilder);
        }

        [GeneratedRegex("<a?:(\\w+):(\\d+)>", RegexOptions.Compiled)]
        private static partial Regex GetEmojiRegex();
    }
}
