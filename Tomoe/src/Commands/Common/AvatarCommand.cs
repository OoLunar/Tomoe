using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Converters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class AvatarCommand : BaseCommand
    {
        private readonly HttpClient HttpClient;

        public AvatarCommand(HttpClient httpClient) => HttpClient = httpClient;

        [Command("avatar"), CommandOverloadPriority(0, true)]
        public async Task ExecuteAsync(CommandContext context, DiscordUser? user = null, [ArgumentConverter<EnumArgumentConverter>] ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
        {
            await context.DelayAsync();

            user ??= context.User;
            if (imageFormat == ImageFormat.Unknown)
            {
                imageFormat = ImageFormat.Auto;
            }

            string avatarUrl = (imageFormat == ImageFormat.Png && imageDimensions is 0) ? user.AvatarUrl : user.GetAvatarUrl(imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions);
            Stream imageStream = await (await HttpClient.GetAsync(avatarUrl)).Content.ReadAsStreamAsync();
            Image image = await Image.LoadAsync(imageStream);
            imageStream.Position = 0; // For image format in the next field.

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"{user.Username}{(user.Username.EndsWith('s') ? "' " : "'s ")}avatar",
                ImageUrl = avatarUrl,
                Color = context.User.BannerColor ?? Optional.FromNoValue<DiscordColor>()
            };

            embedBuilder.AddField("Image Format", Image.DetectFormat(imageStream).Name, true);

            if (imageFormat == ImageFormat.Gif)
            {
                embedBuilder.AddField("Frame Count", image.Frames.Count.ToString("N0"), true);
            }

            embedBuilder.AddField("File Size", imageStream.Length.Bytes().Humanize(), true);
            embedBuilder.AddField("Image Resolution", image.Metadata.ResolutionUnits switch
            {
                PixelResolutionUnit.PixelsPerCentimeter => $"{image.Metadata.HorizontalResolution} x {image.Metadata.VerticalResolution} cm",
                PixelResolutionUnit.PixelsPerInch => $"{image.Metadata.HorizontalResolution / 0.254:#.###} x {image.Metadata.VerticalResolution / 0.254:#.###} cm",
                PixelResolutionUnit.PixelsPerMeter => $"{image.Metadata.HorizontalResolution / 1000:#.###} x {image.Metadata.VerticalResolution / 1000:#.###} cm",
                _ => $"{image.Metadata.HorizontalResolution} x {image.Metadata.VerticalResolution} {image.Metadata.ResolutionUnits.Humanize()}"
            });
            embedBuilder.AddField("Image Dimensions (Size)", $"{image.Width} x {image.Height} pixels.", false);

            await context.EditAsync(new DiscordMessageBuilder().WithEmbed(embedBuilder));
        }
    }
}
