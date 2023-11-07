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
        private readonly HttpClient _httpClient = httpClient;

        public async Task<ImageData> GetImageDataAsync(string url)
        {
            using Stream imageData = await (await _httpClient.GetAsync(url)).Content.ReadAsStreamAsync();
            return new ImageData(imageData);
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
            Format = Image.DetectFormat(imageData).Name;
            imageData.Position = 0;

            Image = Image.Load(imageData);
            Dimensions = $"{Image.Width} x {Image.Height} pixels.";
            Resolution = Image.Metadata.ResolutionUnits switch
            {
                PixelResolutionUnit.PixelsPerCentimeter => $"{Image.Metadata.HorizontalResolution} x {Image.Metadata.VerticalResolution} cm",
                PixelResolutionUnit.PixelsPerInch => $"{Image.Metadata.HorizontalResolution / 0.254:#.###} x {Image.Metadata.VerticalResolution / 0.254:#.###} cm",
                PixelResolutionUnit.PixelsPerMeter => $"{Image.Metadata.HorizontalResolution / 1000:#.###} x {Image.Metadata.VerticalResolution / 1000:#.###} cm",
                _ => $"{Image.Metadata.HorizontalResolution} x {Image.Metadata.VerticalResolution} {Image.Metadata.ResolutionUnits.Humanize()}"
            };

            FileSize = imageData.Length.Bytes().Humanize();
        }
    }
}
