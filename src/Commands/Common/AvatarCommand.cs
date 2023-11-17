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
    [Command("avatar"), TextAlias("pfp", "icon", "profile_picture", "picture")]
    public sealed class AvatarCommand(HttpClient httpClient)
    {
        [Command("user"), SlashCommandTypes(ApplicationCommandType.SlashCommand, ApplicationCommandType.UserContextMenu)]
        public Task UserAsync(CommandContext context, DiscordUser? user = null, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
            => SendAvatarAsync(context, $"{(user ??= context.User).Username}{(user.Username.EndsWith('s') ? "'" : "'s")} Avatar", user.GetAvatarUrl(imageFormat == ImageFormat.Unknown ? ImageFormat.Auto : imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions), user.BannerColor.HasValue && !user.BannerColor.Value.Equals(default(DiscordColor)) ? user.BannerColor.Value : null);

        [Command("webhook"), TextAlias("wh")]
        public async Task WebhookAsync(CommandContext context, [TextMessageReply] DiscordMessage? message = null, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
        {
            if (context is not TextCommandContext textContext)
            {
                await SendAvatarAsync(context, $"{message!.Author.Username}{(message.Author.Username.EndsWith('s') ? "'" : "'s")} Avatar", message.Author.GetAvatarUrl(imageFormat == ImageFormat.Unknown ? ImageFormat.Auto : imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions));
            }
            else if (message is not null)
            {
                await SendAvatarAsync(context, $"{message.Author.Username}{(message.Author.Username.EndsWith('s') ? "'" : "'s")} Avatar", message.Author.GetAvatarUrl(imageFormat == ImageFormat.Unknown ? ImageFormat.Auto : imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions));
            }
            else if (textContext.Message.ReferencedMessage is not null)
            {
                await SendAvatarAsync(context, $"{textContext.Message.ReferencedMessage.Author.Username}{(textContext.Message.ReferencedMessage.Author.Username.EndsWith('s') ? "'" : "'s")} Avatar", textContext.Message.ReferencedMessage.Author.GetAvatarUrl(imageFormat == ImageFormat.Unknown ? ImageFormat.Auto : imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions));
            }
            else
            {
                await context.RespondAsync("Please reply to a message or provide a message link of whose avatar to grab. Additionally ensure the message belongs to this guild.");
            }
        }

        [Command("guild"), TextAlias("member", "server"), RequireGuild]
        public async Task GuildAsync(CommandContext context, DiscordMember? member = null, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
        {
            if (context.Guild is null)
            {
                await context.RespondAsync($"Command `/{context.Command.FullName}` can only be used in a guild.");
            }
            else
            {
                member ??= context.Member!;
                if (member.GuildAvatarHash is null)
                {
                    member = await context.Guild!.GetMemberAsync(member.Id, true);
                }
                await SendAvatarAsync(context, $"{member.Username}{(member.Username.EndsWith('s') ? "'" : "'s")} Guild Avatar", member.GetGuildAvatarUrl(imageFormat == ImageFormat.Unknown ? ImageFormat.Auto : imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions), !member.Color.Equals(default(DiscordColor)) ? member.Color : null);
            }
        }

        private async Task SendAvatarAsync(CommandContext context, string embedTitle, string url, DiscordColor? embedColor = null)
        {
            await context.DeferResponseAsync();
            Stream imageStream = await (await httpClient.GetAsync(url)).Content.ReadAsStreamAsync();
            Image image = await Image.LoadAsync(imageStream);
            imageStream.Position = 0; // For image format in the next field.

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = embedTitle,
                ImageUrl = url,
                Color = embedColor ?? new DiscordColor("#6b73db"),
            };

            embedBuilder.AddField("Image Format", Image.DetectFormat(imageStream).Name, true);

            if (url.Contains("a_"))
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
            await context.EditResponseAsync(new DiscordMessageBuilder().WithEmbed(embedBuilder));
        }
    }
}
