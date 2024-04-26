using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// You spin me right round, baby, right round.
    /// </summary>
    public sealed class RotateCommand
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of <see cref="RotateCommand"/>.
        /// </summary>
        /// <param name="httpClient">Required service for retrieving remote files.</param>
        public RotateCommand(HttpClient httpClient) => _httpClient = httpClient;

        /// <summary>
        /// Rotates an image by the specified degrees.
        /// </summary>
        /// <param name="attachment">The image to rotate.</param>
        /// <param name="degrees">The degrees to rotate the image by. Defaults to 90. Only supports increments of 90.</param>
        [Command("rotate"), Description("Rotates the image by the specified degrees."), RequirePermissions(DiscordPermissions.AttachFiles, DiscordPermissions.None)]
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
