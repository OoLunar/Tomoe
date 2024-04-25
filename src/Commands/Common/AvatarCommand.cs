using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// A command module for retrieving avatars from different sources. <see cref="AvatarCommand"/> is a top-level command.
    /// </summary>
    [Command("avatar"), TextAlias("pfp")]
    public sealed class AvatarCommand
    {
        private readonly ImageUtilities imageUtilitiesService;
        public AvatarCommand(ImageUtilities imageUtilitiesService) => this.imageUtilitiesService = imageUtilitiesService;

        /// <summary>
        /// Fetches the avatar of the requested user.
        /// </summary>
        /// <param name="user">The user to fetch the avatar from. If no user is provided, the command will default to the author.</param>
        /// <param name="imageFormat">The format of the image to fetch. Valid options include: png, gif, jpeg, webp, or auto. Auto will display the gif when available and fallback to png when it's not.</param>
        /// <param name="imageDimensions">A number between 16 and 4096 that represents the dimensions of the image to fetch. If no dimensions are provided, the command will default to 1024. The dimensions must be a power of two.</param>
        /// <returns>Nothing</returns>
        [Command("user"), DefaultGroupCommand, SlashCommandTypes(DiscordApplicationCommandType.SlashCommand, DiscordApplicationCommandType.UserContextMenu)]
        public ValueTask UserAsync(CommandContext context, DiscordUser? user = null, ImageFormat imageFormat = ImageFormat.Auto, ushort imageDimensions = 0)
        {
            user ??= context.User;
            string pluralDisplayName = user.GetDisplayName().PluralizeCorrectly();
            string avatarUrl = user.GetAvatarUrl(imageFormat == ImageFormat.Unknown ? ImageFormat.Auto : imageFormat, imageDimensions == 0 ? (ushort)1024 : imageDimensions);
            DiscordColor? color = user.BannerColor.HasValue && !user.BannerColor.Value.Equals(default(DiscordColor)) ? user.BannerColor.Value : null;
            return SendAvatarAsync(context, $"{pluralDisplayName} Avatar", avatarUrl, color);
        }

        /// <summary>
        /// Fetches the avatar of the user from a message.
        /// </summary>
        /// <param name="message">The message to fetch the avatar from. If no message is provided, the command will default to the message you replied to, if applicable.</param>
        /// <inheritdoc cref="UserAsync(CommandContext, DiscordUser?, ImageFormat, ushort)"/>
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

        /// <summary>
        /// Fetches the server-specific avatar of the requested user.
        /// </summary>
        /// <param name="member">The member to fetch the avatar from. If no member is provided, the command will default to the author. The member must be in the server.</param>
        /// <inheritdoc cref="UserAsync(CommandContext, DiscordUser?, ImageFormat, ushort)"/>
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
            ImageData? imageData = await imageUtilitiesService.GetImageDataAsync(url);
            if (imageData is null)
            {
                // The embed title is set to something like "Lunar's Avatar", so we can just use the embed title.
                await context.RespondAsync(new DiscordMessageBuilder().WithContent($"Failed to retrieve {embedTitle}."));
                return;
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = embedTitle,
                ImageUrl = url,
                Color = embedColor ?? new DiscordColor("#6b73db"),
            };

            embedBuilder.AddField("Image Format", imageData.Format, true);
            if (url.Contains("a_"))
            {
                embedBuilder.AddField("Frame Count", imageData.FrameCount.ToString("N0", CultureInfo.InvariantCulture), true);
            }

            embedBuilder.AddField("File Size", imageData.FileSize, true);
            embedBuilder.AddField("Image Resolution", imageData.Resolution, false);
            embedBuilder.AddField("Image Dimensions (Size)", imageData.Dimensions, false);
            await context.RespondAsync(new DiscordMessageBuilder().AddEmbed(embedBuilder));
        }
    }
}
