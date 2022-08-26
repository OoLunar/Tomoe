using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class GuildIcon : BaseCommandModule
    {
        [Command("guild_icon"), Description("Grabs the guild's icon."), Aliases("server_icon", "icon"), RequireGuild]
        public Task GuildIconAsync(CommandContext context, ushort imageSize = 4096, ImageFormat imageFormat = ImageFormat.Auto) => GuildIconAsync(context, context.Guild.Id, imageSize, imageFormat);

        [Command("guild_icon")]
        public async Task GuildIconAsync(CommandContext context, ulong guildId, ushort imageSize = 4096, ImageFormat imageFormat = ImageFormat.Auto)
        {
            if (context.Client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
            {
                await context.RespondAsync(guild.GetIconUrl(imageFormat, imageSize));
                return;
            }

            DiscordGuildPreview guildPreview = await context.Client.GetGuildPreviewAsync(guildId);
            string? iconUrl = GetIconUrl(guildPreview, imageFormat, imageSize);
            if (iconUrl == null)
            {
                await context.RespondAsync("Could not find an icon for the guild.");
                return;
            }

            await context.RespondAsync(iconUrl);
        }

        /// <summary>
        /// Gets guild's icon URL, in requested format and size.
        /// </summary>
        /// <param name="imageFormat">The image format of the icon to get.</param>
        /// <param name="imageSize">The maximum size of the icon. Must be a power of two, minimum 16, maximum 4096.</param>
        /// <returns>The URL of the guild's icon.</returns>
        public string? GetIconUrl(DiscordGuildPreview guildPreview, ImageFormat imageFormat, ushort imageSize = 1024)
        {
            if (string.IsNullOrWhiteSpace(guildPreview.Icon))
            {
                return null;
            }

            if (imageFormat == ImageFormat.Unknown)
            {
                throw new ArgumentException("You must specify valid image format.", nameof(imageFormat));
            }
            // Makes sure the image size is in between Discord's allowed range.
            else if (imageSize is < 16 or > 4096)
            {
                throw new ArgumentOutOfRangeException(nameof(imageSize), imageSize, "Image Size is not in between 16 and 4096.");
            }
            // Checks to see if the image size is not a power of two.
            else if (!(imageSize is not 0 && (imageSize & (imageSize - 1)) is 0))
            {
                throw new ArgumentOutOfRangeException(nameof(imageSize), imageSize, "Image size is not a power of two.");
            }

            // Get the string variants of the method parameters to use in the urls.
            string stringImageFormat = imageFormat switch
            {
                ImageFormat.Gif => "gif",
                ImageFormat.Jpeg => "jpg",
                ImageFormat.Png => "png",
                ImageFormat.WebP => "webp",
                ImageFormat.Auto => !string.IsNullOrWhiteSpace(guildPreview.Icon) ? (guildPreview.Icon.StartsWith("a_") ? "gif" : "png") : "png",
                _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
            };

            string stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);
            return $"https://cdn.discordapp.com/icons/{guildPreview.Id}/{guildPreview.Icon}.{stringImageFormat}?size={stringImageSize}";
        }
    }
}
