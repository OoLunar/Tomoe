using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Converters
{
    public sealed class ImageFormatConverter : IArgumentConverter<ImageFormat>
    {
        public Task<Optional<ImageFormat>> ConvertAsync(string value, CommandContext context)
        {
            ImageFormat? result = (ImageFormat?)value.DehumanizeTo(typeof(ImageFormat), OnNoMatch.ReturnsNull);
            return result == null ? Task.FromResult(Optional.FromNoValue<ImageFormat>()) : Task.FromResult(Optional.FromValue(result.Value));
        }
    }
}
