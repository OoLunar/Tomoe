using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class HexCommand : BaseCommand
    {
        [Command("hex")]
        public static Task ExecuteAsync(CommandContext context, string hexCode)
        {
            ReadOnlySpan<char> hexCodeSpan = hexCode.AsSpan();
            if (!IsValidHex(ref hexCodeSpan))
            {
                return context.ReplyAsync($"#{hexCode} is not a valid hex code. Please send a valid HTML hex code, optionally with a leading '#' or alpha channel.");
            }

            System.Drawing.Color color = ColorTranslator.FromHtml($"#{hexCodeSpan}");
            Image<Rgba32> image = new(256, 256);
            image.Mutate(x => x.BackgroundColor(new Rgba32(color.R, color.G, color.B, color.A)));

            MemoryStream stream = new();
            image.SaveAsPng(stream);
            stream.Position = 0;

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .WithContent($"Hex: {hexCode}\nRGBA: {color.R}, {color.G}, {color.B}, {color.A}")
                .AddFile($"{color.R}{color.G}{color.B}{color.A}.png", stream)
                .WithEmbed(new DiscordEmbedBuilder()
                {
                    Color = new DiscordColor(color.R, color.G, color.B),
                    ImageUrl = $"attachment://{color.R}{color.G}{color.B}{color.A}.png"
                });

            return context.ReplyAsync(messageBuilder);
        }

        private static bool IsValidHex(ref ReadOnlySpan<char> hexString)
        {
            // Check if the first character is a '#' and remove it
            if (hexString[0] == '#')
            {
                hexString = hexString[1..];
            }

            // Check if the length is 8, 4, 6, or 3. Other lengths are not compliant with hex codes
            if (hexString.Length is not 8 and not 4 and not 6 and not 3)
            {
                return false;
            }

            // Check if the string contains only valid hex characters
            for (int i = 0; i < hexString.Length; i++)
            {
                if (hexString[i] is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'))
                {
                    // If the character is a valid hex character
                    continue;
                }

                // If the character is not a valid hex character, return false
                return false;
            }

            // If the string is valid, return true
            return true;
        }

    }
}
