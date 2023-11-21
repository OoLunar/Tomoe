using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Humanizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace OoLunar.Tomoe.Services
{
    public sealed class ImageUtilitiesService(HttpClient httpClient)
    {
        public async ValueTask<ImageData?> GetImageDataAsync(string url)
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
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
