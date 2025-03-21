using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace OoLunar.Tomoe
{
    /// <summary>
    /// Represents format of an image.
    /// </summary>
    public enum ImageFormat : int
    {
        Png,
        Gif,
        Jpeg,
        WebP,
        Auto,
        Unknown
    }

    public sealed class ImageUtilities
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<ImageUtilities> logger;

        public ImageUtilities(HttpClient httpClient, ILogger<ImageUtilities> logger)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.logger = logger ?? NullLogger<ImageUtilities>.Instance;
        }

        public async ValueTask<ImageData?> GetImageDataAsync(string url)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Http Error {HttpError}. Failed to get image data from {Url}.", (int)response.StatusCode, url);
                return null;
            }

            using Stream imageDataStream = await response.Content.ReadAsStreamAsync();
            return new ImageData(imageDataStream);
        }
    }

    public sealed class ImageData
    {
        public Image Image { get; init; }
        public string Dimensions { get; init; }
        public string Resolution { get; init; }
        public string FileSize { get; init; }
        public string Format { get; init; }
        public int FrameCount => Image.Frames.Count;

        public ImageData(Stream imageData)
        {
            Image = Image.Load(imageData);

            imageData.Position = 0;
            Format = Image.DetectFormat(imageData).Name;
            Dimensions = $"{Image.Width} x {Image.Height} pixels.";
            Resolution = Image.Metadata.ResolutionUnits switch
            {
                PixelResolutionUnit.PixelsPerCentimeter => $"{Image.Metadata.HorizontalResolution} x {Image.Metadata.VerticalResolution} cm",
                PixelResolutionUnit.PixelsPerInch => $"{Math.Round(Image.Metadata.HorizontalResolution / 0.254, MidpointRounding.AwayFromZero)} x {Math.Round(Image.Metadata.VerticalResolution / 0.254, MidpointRounding.AwayFromZero)} cm",
                PixelResolutionUnit.PixelsPerMeter => $"{Math.Round(Image.Metadata.HorizontalResolution / 1000, MidpointRounding.AwayFromZero)} x {Math.Round(Image.Metadata.VerticalResolution / 1000, MidpointRounding.AwayFromZero)} cm",
                _ => $"{Image.Metadata.HorizontalResolution} x {Image.Metadata.VerticalResolution} {Image.Metadata.ResolutionUnits.Humanize()}"
            };

            FileSize = imageData.Length.Bytes().Humanize(CultureInfo.InvariantCulture);
        }
    }
}
