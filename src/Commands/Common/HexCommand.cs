using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// I cast magic missile at the darkness!
    /// </summary>
    public static partial class HexCommand
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Value")]
        private static extern long _getValue(ref System.Drawing.Color color);

        /// <summary>
        /// Returns information about the provided hex color code.
        /// </summary>
        /// <param name="hexCode">Which HTML hex code to get information about.</param>
        [Command("hex")]
        public static ValueTask ExecuteAsync(CommandContext context, string hexCode)
        {
            if (!IsValidHex(hexCode))
            {
                return context.RespondAsync($"`#{Formatter.Sanitize(hexCode)}` is not a valid hex code. Please send a valid HTML hex code, optionally with a leading '#' or alpha channel.");
            }

            System.Drawing.Color color = ColorTranslator.FromHtml($"#{hexCode.TrimStart('#')}");
            Image<Rgba32> image = new(256, 256);
            image.Mutate(x => x.BackgroundColor(new Rgba32(color.R, color.G, color.B, color.A)));

            MemoryStream stream = new();
            image.SaveAsPng(stream);
            stream.Position = 0;

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .WithContent($"Hex: {_getValue(ref color):X}\nRGBA: {color.R}, {color.G}, {color.B}, {color.A}")
                .AddFile($"{color.R}{color.G}{color.B}{color.A}.png", stream)
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Color = new DiscordColor(color.R, color.G, color.B),
                    ImageUrl = $"attachment://{color.R}{color.G}{color.B}{color.A}.png"
                });

            return context.RespondAsync(messageBuilder);
        }

        private static bool IsValidHex(ReadOnlySpan<char> hexString)
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
