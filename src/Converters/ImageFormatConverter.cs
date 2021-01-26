using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Tomoe
{
	public class ImageFormatConverter : IArgumentConverter<ImageFormat>
	{
		public Task<Optional<ImageFormat>> ConvertAsync(string value, CommandContext ctx)
		{
			switch (value.ToLowerInvariant())
			{
				case "png":
					return Task.FromResult(Optional.FromValue(ImageFormat.Png));
				case "jpeg":
					return Task.FromResult(Optional.FromValue(ImageFormat.Jpeg));
				case "webp":
					return Task.FromResult(Optional.FromValue(ImageFormat.WebP));
				case "gif":
					return Task.FromResult(Optional.FromValue(ImageFormat.Gif));
				case "unknown":
				case "auto":
					return Task.FromResult(Optional.FromValue(ImageFormat.Auto));
				default:
					return Task.FromResult(Optional.FromNoValue<ImageFormat>());
			}
		}
	}
}
