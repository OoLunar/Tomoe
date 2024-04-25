using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Sends the icon of a guild.
    /// </summary>
    public static class GuildIconCommand
    {
        /// <summary>
        /// Sends the guild icon in the requested format and size.
        /// </summary>
        /// <param name="imageFormat">The image format of the icon to get.</param>
        /// <param name="imageDimensions">The maximum size of the icon. Must be a power of two, minimum 16, maximum 4096.</param>
        /// <param name="guildId">The ID of the guild to get the icon from. Defaults to the current server.</param>
        /// <returns></returns>
        [Command("guild_icon"), TextAlias("guild_picture")]
        public static async ValueTask ExecuteAsync(CommandContext context, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0, ulong guildId = 0)
        {
            if (guildId == 0)
            {
                if (context.Guild is null)
                {
                    await context.RespondAsync("Please provide a guild id.");
                    return;
                }

                guildId = context.Guild.Id;
            }

            if (imageFormat == ImageFormat.Unknown)
            {
                imageFormat = ImageFormat.Auto;
            }

            if (imageDimensions == 0)
            {
                imageDimensions = 1024;
            }

            if (context.Client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
            {
                await context.RespondAsync(guild.GetIconUrl(imageFormat, imageDimensions));
                return;
            }

            DiscordGuildPreview guildPreview = await context.Client.GetGuildPreviewAsync(guildId);
            string? iconUrl = GetIconUrl(guildPreview, imageFormat, imageDimensions);
            if (iconUrl == null)
            {
                await context.RespondAsync("Could not find an icon for the guild.");
            }
            else
            {
                await context.RespondAsync(iconUrl);
            }
        }

        /// <summary>
        /// Gets guild's icon URL, in requested format and size.
        /// </summary>
        /// <param name="imageFormat">The image format of the icon to get.</param>
        /// <param name="imageSize">The maximum size of the icon. Must be a power of two, minimum 16, maximum 4096.</param>
        /// <returns>The URL of the guild's icon.</returns>
        public static string? GetIconUrl(DiscordGuildPreview guildPreview, ImageFormat imageFormat, ushort imageSize = 1024)
        {
            if (string.IsNullOrWhiteSpace(guildPreview.Icon))
            {
                return null;
            }
            else if (imageFormat == ImageFormat.Unknown)
            {
                imageFormat = ImageFormat.Auto;
            }

            // Makes sure the image size is in between Discord's allowed range.
            if (imageSize is < 16 or > 4096)
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
                ImageFormat.Auto => !string.IsNullOrWhiteSpace(guildPreview.Icon) ? (guildPreview.Icon.StartsWith("a_", false, CultureInfo.InvariantCulture) ? "gif" : "png") : "png",
                _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
            };

            string stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);
            return $"https://cdn.discordapp.com/icons/{guildPreview.Id}/{guildPreview.Icon}.{stringImageFormat}?size={stringImageSize}";
        }
    }
}
