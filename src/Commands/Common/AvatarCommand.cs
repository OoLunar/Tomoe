using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands.Attributes;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("avatar"), TextAlias("pfp")]
    public sealed class AvatarCommand(HttpClient httpClient)
    {
        [Command("user"), SlashCommandTypes(ApplicationCommandType.SlashCommand, ApplicationCommandType.UserContextMenu)]
        public ValueTask UserAsync(CommandContext context, DiscordUser? user = null, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
        {
            user ??= context.User;
            string pluralDisplayName = user.GetDisplayName().PluralizeCorrectly();
            string avatarUrl = user.GetAvatarUrl(imageFormat == ImageFormat.Unknown ? ImageFormat.Auto : imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions);
            DiscordColor? color = user.BannerColor.HasValue && !user.BannerColor.Value.Equals(default(DiscordColor)) ? user.BannerColor.Value : null;
            return SendAvatarAsync(context, $"{pluralDisplayName} Avatar", avatarUrl, color);
        }

        [Command("webhook"), TextAlias("wh")]
        public ValueTask WebhookAsync(CommandContext context, [TextMessageReply] DiscordMessage? message = null, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
        {
            if (message is null)
            {
                if (context is not TextCommandContext textCommandContext)
                {
                    return context.RespondAsync("Please provide a message link of whose avatar to grab. Additionally ensure the message belongs to this guild.");
                }
                else if (textCommandContext.Message.ReferencedMessage is null)
                {
                    return context.RespondAsync("Please reply to a message or provide a message link of whose avatar to grab. Additionally ensure the message belongs to this guild.");
                }

                message = textCommandContext.Message.ReferencedMessage;
            }

            return UserAsync(context, message.Author, imageFormat, imageDimensions);
        }

        [Command("guild"), TextAlias("member", "server"), RequireGuild]
        public async ValueTask GuildAsync(CommandContext context, DiscordMember? member = null, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
        {
            member ??= context.Member!;
            if (member.GuildAvatarHash is null)
            {
                member = await context.Guild!.GetMemberAsync(member.Id, true);
            }

            await UserAsync(context, member, imageFormat, imageDimensions);
        }

        private async ValueTask SendAvatarAsync(CommandContext context, string embedTitle, string url, DiscordColor? embedColor = null)
        {
            await context.DeferResponseAsync();
            using HttpResponseMessage response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                // The embed title is set to something like "Lunar's Avatar", so we can just use the embed title.
                await context.EditResponseAsync(new DiscordMessageBuilder().WithContent($"Failed to retrieve {embedTitle}. HTTP Error `{response.StatusCode}`."));
                return;
            }

            using Stream imageStream = await response.Content.ReadAsStreamAsync();
            Image image = await Image.LoadAsync(imageStream);
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = embedTitle,
                ImageUrl = url,
                Color = embedColor ?? new DiscordColor("#6b73db"),
            };

            imageStream.Position = 0; // For image format
            embedBuilder.AddField("Image Format", Image.DetectFormat(imageStream).Name, true);
            if (url.Contains("a_"))
            {
                embedBuilder.AddField("Frame Count", image.Frames.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            }

            embedBuilder.AddField("File Size", imageStream.Length.Bytes().Humanize(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Image Resolution", image.Metadata.ResolutionUnits switch
            {
                PixelResolutionUnit.PixelsPerCentimeter => $"{image.Metadata.HorizontalResolution} x {image.Metadata.VerticalResolution} cm",
                PixelResolutionUnit.PixelsPerInch => $"{image.Metadata.HorizontalResolution / 0.254:#.###} x {image.Metadata.VerticalResolution / 0.254:#.###} cm",
                PixelResolutionUnit.PixelsPerMeter => $"{image.Metadata.HorizontalResolution / 1000:#.###} x {image.Metadata.VerticalResolution / 1000:#.###} cm",
                _ => $"{image.Metadata.HorizontalResolution} x {image.Metadata.VerticalResolution} {image.Metadata.ResolutionUnits.Humanize()}"
            });

            embedBuilder.AddField("Image Dimensions (Size)", $"{image.Width} x {image.Height} pixels.", false);
            await context.EditResponseAsync(new DiscordMessageBuilder().WithEmbed(embedBuilder));
        }
    }
}
