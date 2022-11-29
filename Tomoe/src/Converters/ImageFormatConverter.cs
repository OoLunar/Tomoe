using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Converters
{
    /// <summary>
    /// Converts <see cref="ImageFormat"/> from a string.
    /// </summary>
    public sealed class ImageFormatConverter : IArgumentConverter<ImageFormat>
    {
        /// <summary>
        /// Converts the string to an <see cref="ImageFormat"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="context">The context of the value.</param>
        /// <returns>An optional of whether the image format was successfully parsed or not.</returns>
        public Task<Optional<ImageFormat>> ConvertAsync(string value, CommandContext context)
        {
            // Use the humanizer library to parse the string into an enum. Allows for all formats of the enum to be used. (Png, PNG, png, 3, etc.)
            ImageFormat? result = (ImageFormat?)value.DehumanizeTo(typeof(ImageFormat), OnNoMatch.ReturnsNull);
            return result == null ? Task.FromResult(Optional.FromNoValue<ImageFormat>()) : Task.FromResult(Optional.FromValue(result.Value));
        }
    }
}
