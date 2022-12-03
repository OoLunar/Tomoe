using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Commands.System.Commands;
using OoLunar.DSharpPlus.CommandAll.Converters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("avatar")]
    public sealed class AvatarCommand : BaseCommand
    {
        private readonly HttpClient HttpClient;

        public AvatarCommand(HttpClient httpClient) => HttpClient = httpClient;

        public override async Task BeforeExecutionAsync(CommandContext context)
        {
            ushort imageDimensions = (ushort)context.NamedArguments.Last().Value!;
            if (imageDimensions == 0)
            {
                imageDimensions = 1024;
            }
            else if (imageDimensions < 16 || imageDimensions > 4096 || (imageDimensions & (imageDimensions - 1)) != 0)
            {
                await context.ReplyAsync("The image dimensions must be `16`, `32`, `64`, `128`, `256`, `512`, `1024`, `2048` or `4096`");
                return;
            }

            ImageFormat imageFormat = (ImageFormat)context.NamedArguments.Skip(1).First().Value!;
            if (imageFormat is ImageFormat.Unknown)
            {
                imageFormat = ImageFormat.Png;
            }

            DiscordEmbedBuilder embedBuilder = new() { Color = new DiscordColor("#6b73db") };
            string url;
            KeyValuePair<CommandParameter, object> firstArgument = context.NamedArguments.First()!;
            Type parameterType = Nullable.GetUnderlyingType(firstArgument.Key.ParameterInfo.ParameterType) ?? firstArgument.Key.ParameterInfo.ParameterType;
            if (parameterType == typeof(DiscordUser))
            {
                DiscordUser user = firstArgument.Value as DiscordUser ?? context.User;
                embedBuilder.Title = $"{user.Username}{(user.Username.EndsWith('s') ? "'" : "'s")} Avatar";

                // If the user was obtained from an API request, attempt to grab their banner color.
                if (user.BannerColor.HasValue && !user.BannerColor.Value.Equals(default(DiscordColor)))
                {
                    embedBuilder.Color = user.BannerColor.Value;
                }

                url = user.GetAvatarUrl(imageFormat, imageDimensions);
            }
            else if (parameterType == typeof(DiscordMember))
            {
                if (context.Guild is null)
                {
                    await context.ReplyAsync($"`/{context.CurrentCommand.FullName}` can only be used in a guild!");
                    return;
                }
                else
                {
                    DiscordMember member = firstArgument.Value as DiscordMember ?? context.Member!;
                    embedBuilder.Title = $"{member.Username}{(member.Username.EndsWith('s') ? "'" : "'s")} Guild Avatar";
                    if (!member.Color.Equals(default(DiscordColor)))
                    {
                        embedBuilder.Color = member.Color;
                    }

                    url = member.GetGuildAvatarUrl(imageFormat, imageDimensions);
                }
            }
            else if (parameterType == typeof(DiscordMessage))
            {
                if (context.IsSlashCommand || firstArgument.Value is not null)
                {
                    DiscordMessage message = (DiscordMessage)firstArgument.Value;
                    embedBuilder.Title = $"{message.Author.Username}{(message.Author.Username.EndsWith('s') ? "'" : "'s")} Avatar";
                    url = message.Author.GetAvatarUrl(imageFormat, imageDimensions);
                }
                else
                {
                    if (context.Message!.ReferencedMessage is null)
                    {
                        await context.ReplyAsync("Please reply to a message or provide a message link of whose avatar to grab.");
                        return;
                    }
                    else
                    {
                        embedBuilder.Title = $"{context.Message.ReferencedMessage.Author.Username}{(context.Message.ReferencedMessage.Author.Username.EndsWith('s') ? "'" : "'s")} Avatar";
                        url = context.Message.ReferencedMessage.Author.GetAvatarUrl(imageFormat, imageDimensions);
                    }
                }
            }
            else
            {
                throw new NotImplementedException(context.CurrentCommand.FullName);
            }

            await SendAvatarAsync(context, embedBuilder, url);
        }

        [Command("user")]
        public static Task UserAsync(CommandContext context, DiscordUser? user = null, [ArgumentConverter<EnumArgumentConverter>] ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0) => Task.CompletedTask;

        [Command("webhook")]
        public static Task WebhookAsync(CommandContext context, DiscordMessage? message = null, [ArgumentConverter<EnumArgumentConverter>] ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0) => Task.CompletedTask;

        [Command("guild")]
        public static Task GuildAsync(CommandContext context, DiscordMember? member = null, [ArgumentConverter<EnumArgumentConverter>] ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0) => Task.CompletedTask;

        private async Task SendAvatarAsync(CommandContext context, DiscordEmbedBuilder embedBuilder, string url)
        {
            await context.DelayAsync();
            Stream imageStream = await (await HttpClient.GetAsync(url)).Content.ReadAsStreamAsync();
            Image image = await Image.LoadAsync(imageStream);
            imageStream.Position = 0; // For image format in the next field.

            embedBuilder.ImageUrl = url;
            embedBuilder.AddField("Image Format", Image.DetectFormat(imageStream).Name, true);
            embedBuilder.AddField("Frame Count", image.Frames.Count.ToString("N0"), true);
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
