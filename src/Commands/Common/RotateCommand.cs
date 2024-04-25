using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class RotateCommand
    {
        private readonly HttpClient _httpClient;
        public RotateCommand(HttpClient httpClient) => _httpClient = httpClient;

        [Command("rotate")]
        [Description("Rotates the image by the specified degrees.")]
        public async ValueTask ExecuteAsync(CommandContext context, [TextMessageReply] DiscordAttachment attachment, float degrees = 90.0f)
        {
            // Download the image, rotate it, and send it back
            if (attachment.MediaType is not null && !attachment.MediaType.Contains("image", StringComparison.OrdinalIgnoreCase))
            {
                await context.RespondAsync("The attachment must be an image.");
                return;
            }

            await context.DeferResponseAsync();
            using HttpResponseMessage response = await _httpClient.GetAsync(attachment.Url);
            response.EnsureSuccessStatusCode();
            if ((response.Headers.TryGetValues("Content-Type", out IEnumerable<string>? values) && !values.Any(value => value.Contains("image", StringComparison.OrdinalIgnoreCase)))
                || !response.Content.Headers.ContentType?.MediaType?.Contains("image", StringComparison.OrdinalIgnoreCase) is true)
            {
                await context.RespondAsync("The attachment must be an image.");
                return;
            }

            Image<Rgba32> image = await Image.LoadAsync<Rgba32>(await response.Content.ReadAsStreamAsync());
            image.Mutate(x => x.Rotate(degrees));
            await using MemoryStream stream = new();
            await image.SaveAsPngAsync(stream);
            stream.Position = 0;
            await context.RespondAsync(new DiscordMessageBuilder().AddFile("rotated.png", stream));
        }
    }
}
