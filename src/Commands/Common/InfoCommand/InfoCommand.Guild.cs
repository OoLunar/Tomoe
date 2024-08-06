using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class InfoCommand
    {
        /// <summary>
        /// Sends information about the server.
        /// </summary>
        /// <param name="guildId">The id of the guild to get information about. Leave empty to get information about the current server.</param>
        [Command("guild"), TextAlias("server")]
        public async Task GuildInfoAsync(CommandContext context, ulong? guildId = null)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#6b73db"),
                Author = new()
                {
                    Name = context.User.GetDisplayName(),
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
            };

            if (guildId is not null)
            {
                // If the guild id was specified but the bot is not in the guild, return a guild preview.
                if (!context.Client.Guilds.TryGetValue(guildId.Value, out DiscordGuild? guild))
                {
                    DiscordGuildPreview guildPreview;
                    try
                    {
                        guildPreview = await context.Client.GetGuildPreviewAsync(guildId.Value);
                    }
                    catch (DiscordException)
                    {
                        await context.RespondAsync($"That guild is a private server. Since I am not in the guild, I cannot return any information about it.");
                        return;
                    }

                    embedBuilder.Title = guildPreview.Name;
                    embedBuilder.Footer = new() { IconUrl = $"https://cdn.discordapp.com/splashes/{guildId}/{guildPreview.Splash}.png" };
                    if (guildPreview.Icon is not null)
                    {
                        embedBuilder.Thumbnail = new() { Url = $"https://cdn.discordapp.com/icons/{guildId}/{guildPreview.Icon}.{(guildPreview.Icon.StartsWith("a_") ? "gif" : "png")}" };
                    }

                    string features = string.Join(", ", guildPreview.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
                    embedBuilder.AddField("Server Description", string.IsNullOrWhiteSpace(guildPreview.Description) ? "No description." : guildPreview.Description, false);
                    embedBuilder.AddField("Server Id", Formatter.InlineCode(guildPreview.Id.ToString(CultureInfo.InvariantCulture)), true);
                    embedBuilder.AddField("Created At", Formatter.Timestamp(guildPreview.CreationTimestamp.UtcDateTime, TimestampFormat.RelativeTime), true);
                    embedBuilder.AddField("Emoji Count", guildPreview.Emojis.Count.ToString("N0", CultureInfo.InvariantCulture), true);
                    embedBuilder.AddField("Approximate Member Count", guildPreview.ApproximateMemberCount.ToString("N0", CultureInfo.InvariantCulture), true);
                    embedBuilder.AddField("Features", string.IsNullOrWhiteSpace(features) ? "None" : features, false);
                }
                // The bot is in the guild, so return the guild info.
                else
                {
                    await ProvideGuildInfoAsync(embedBuilder, guild);
                }
            }
            // The guild id was not specified, so return the guild info for the guild the command was executed in.
            else if (context.Guild is not null)
            {
                await ProvideGuildInfoAsync(embedBuilder, context.Guild);
            }
            // The command was executed in a DM without a guild id, so return an error.
            else
            {
                await context.RespondAsync($"Command `/{context.Command.FullName}` should be used in a server or you should provide a guild id.");
                return;
            }

            await context.RespondAsync(embedBuilder);
        }

        private async Task ProvideGuildInfoAsync(DiscordEmbedBuilder embedBuilder, DiscordGuild guild)
        {
            if (guild.IconUrl is not null)
            {
                embedBuilder.Thumbnail = new() { Url = guild.GetIconUrl(ImageFormat.Auto, 4096) };
            }

            string features = string.Join(", ", guild.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
            embedBuilder.AddField("Server Description", string.IsNullOrWhiteSpace(guild.Description) ? "No description." : guild.Description, false);
            embedBuilder.AddField("Owner", $"<@{guild.OwnerId}>", true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(guild.CreationTimestamp.UtcDateTime, TimestampFormat.RelativeTime), true);
            embedBuilder.AddField("Server Id", Formatter.InlineCode(guild.Id.ToString(CultureInfo.InvariantCulture)), true);
            embedBuilder.AddField("Emoji Count", guild.Emojis.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Role Count", guild.Roles.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Sticker Count", guild.Stickers.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Member Count", guild.MemberCount.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Currently Scheduled Events", (guild.ScheduledEvents.Count == 0 ? (await guild.GetEventsAsync(false)).Count : guild.ScheduledEvents.Count).ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Features", string.IsNullOrWhiteSpace(features) ? "None" : features, false);
        }
    }
}
