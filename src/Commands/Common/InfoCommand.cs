using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("info")]
    public sealed partial class InfoCommand
    {
        private static readonly string _operatingSystem = $"{Environment.OSVersion} {RuntimeInformation.OSArchitecture.ToString().ToLower(CultureInfo.InvariantCulture)}";
        private static readonly string _botVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        private static readonly string _dSharpPlusVersion = typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        [GeneratedRegex(", (?=[^,]*$)", RegexOptions.Compiled)]
        private static partial Regex _getLastCommaRegex();

        [GeneratedRegex("<a?:(\\w+):(\\d+)>", RegexOptions.Compiled)]
        private static partial Regex _getEmojiRegex();

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_UnicodeEmojis")]
        private static extern IReadOnlyDictionary<string, string> _unicodeEmojis(DiscordEmoji emoji);

        private readonly ImageUtilities _imageUtilitiesService;
        public InfoCommand(ImageUtilities imageUtilitiesService) => _imageUtilitiesService = imageUtilitiesService ?? throw new ArgumentNullException(nameof(imageUtilitiesService));

        [Command("bot"), DefaultGroupCommand]
        public static async ValueTask BotInfoAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#6b73db")
            };

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();

            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Process Memory", currentProcess.WorkingSet64.Bytes().ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Runtime Version", RuntimeInformation.FrameworkDescription, true);

            embedBuilder.AddField("Operating System", _operatingSystem, true);
            embedBuilder.AddField("Uptime", _getLastCommaRegex().Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);
            embedBuilder.AddField("Websocket Ping", _getLastCommaRegex().Replace(context.Client.Ping.Milliseconds().Humanize(3), " and "), true);

            embedBuilder.AddField("Guild Count", (await GuildMemberModel.CountGuildsAsync()).ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("User Count", (await GuildMemberModel.CountMembersAsync()).ToString("N0", CultureInfo.InvariantCulture), true);

            StringBuilder stringBuilder = new();
            stringBuilder.Append(context.Client.CurrentUser.Mention);
            stringBuilder.Append(", `");
            stringBuilder.Append(((DefaultPrefixResolver)context.Extension.GetProcessor<TextCommandProcessor>().Configuration.PrefixResolver.Target!).Prefix);
            stringBuilder.Append("`, `/`");
            embedBuilder.AddField("Prefixes", stringBuilder.ToString(), true);
            embedBuilder.AddField("Bot Version", _botVersion, true);
            embedBuilder.AddField("DSharpPlus Library Version", _dSharpPlusVersion, true);

            await context.RespondAsync(embedBuilder);
        }

        [Command("user")]
        public static async Task UserInfoAsync(CommandContext context, DiscordUser? user = null)
        {
            user ??= context.User;
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Info about {user.GetDisplayName()}",
                Thumbnail = new() { Url = user.AvatarUrl },
                Color = new DiscordColor("#6b73db")
            };

            embedBuilder.AddField("Mention", user.Mention, true);
            if (user is DiscordMember member)
            {
                if (!member.Color.Equals(default(DiscordColor)))
                {
                    embedBuilder.Color = member.Color;
                }

                GuildMemberModel? memberModel = await GuildMemberModel.FindMemberAsync(context.User.Id, context.Guild!.Id);
                if (memberModel is not null && memberModel.FirstJoined != member.JoinedAt.UtcDateTime)
                {
                    embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), false);
                    embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
                    embedBuilder.AddField("First joined the Server", Formatter.Timestamp(memberModel.FirstJoined, TimestampFormat.RelativeTime), true);
                }
                else
                {
                    embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), true);
                    // ZWS field
                    embedBuilder.AddField("\u200B", "\u200B", true);
                    embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
                }

                embedBuilder.AddField("Recently joined the Server", Formatter.Timestamp(member.JoinedAt, TimestampFormat.RelativeTime), true);
                embedBuilder.AddField("Roles", member.Roles.Any() ? string.Join('\n', member.Roles.OrderByDescending(role => role.Position).Select(role => $"- {role.Mention}")) : "None", false);
            }
            else
            {
                embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), true);
                embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
            }

            await context.RespondAsync(embedBuilder);
        }

        [Command("role"), RequireGuild]
        public static async Task RoleInfoAsync(CommandContext context, DiscordRole role)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Role Info for {role.Name}",
                Author = new()
                {
                    Name = context.Member!.DisplayName,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Color = role.Color.Value == 0x000000 ? null : role.Color
            };
            embedBuilder.AddField("Color", role.Color.ToString(), true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(role.CreationTimestamp.UtcDateTime, TimestampFormat.LongDateTime), true);
            embedBuilder.AddField("Hoisted", role.IsHoisted.ToString(), true);
            embedBuilder.AddField("Is Managed", role.IsManaged.ToString(), true);
            embedBuilder.AddField("Is Mentionable", role.IsMentionable.ToString(), true);
            embedBuilder.AddField("Role Id", Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture)), true);
            embedBuilder.AddField("Role Name", role.Name, true);
            embedBuilder.AddField("Role Position", role.Position.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Permissions", role.Permissions == Permissions.None ? "No permissions." : role.Permissions.ToPermissionString() + ".", false);

            int fieldCharCount = 0;
            List<string> memberMentions = [];
            await foreach (GuildMemberModel member in GuildMemberModel.GetMembersWithRoleAsync(context.Guild!.Id, role.Id))
            {
                string mention = $"<@{member.UserId.ToString(CultureInfo.InvariantCulture)}>";
                fieldCharCount += mention.Length;
                if (fieldCharCount > 1024)
                {
                    break;
                }

                memberMentions.Add(mention);
            }

            memberMentions.Sort(string.CompareOrdinal);
            embedBuilder.AddField("Member Count", memberMentions.Count.ToString("N0", CultureInfo.InvariantCulture), false);
            embedBuilder.AddField("Members", string.Join(", ", memberMentions.DefaultIfEmpty("None")), true);
            await context.RespondAsync(embedBuilder);
        }

        [Command("emoji")]
        public async Task EmojiInfoAsync(CommandContext context, string emoji)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#6b73db")
            };

            // We parse the emoji by hand in case if the bot doesn't have access to the emoji.
            if (DiscordEmoji.TryFromUnicode(context.Client, emoji, out DiscordEmoji? discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", _unicodeEmojis(null!).First(x => x.Value == discordEmoji.Name).Key.Replace(":", "\\:"), true);
                embedBuilder.AddField("Unicode", $"\\{discordEmoji.Name}", true);
                embedBuilder.ImageUrl = $"https://raw.githubusercontent.com/twitter/twemoji/master/assets/72x72/{char.ConvertToUtf32(discordEmoji.Name, 0).ToString("X4", CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture)}.png";
            }
            else if (DiscordEmoji.TryFromName(context.Client, emoji, out discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", discordEmoji.Name, true);
                embedBuilder.AddField("Emoji ID", $"`{discordEmoji.Id.ToString(CultureInfo.InvariantCulture)}`", true);
                embedBuilder.ImageUrl = discordEmoji.Url;
            }
            else
            {
                Match match = _getEmojiRegex().Match(emoji);
                if (!match.Success)
                {
                    await context.RespondAsync("Invalid emoji.");
                    return;
                }

                embedBuilder.AddField("Emoji Name", match.Groups[1].Value, true);
                embedBuilder.AddField("Emoji ID", $"`{match.Groups[2].Value}`", true);
                embedBuilder.ImageUrl = $"https://cdn.discordapp.com/emojis/{match.Groups[2].Value}.png";
            }

            // ZWS field
            embedBuilder.AddField("\u200B", "\u200B", true);
            embedBuilder.AddField("Emoji URL", Formatter.MaskedUrl("Link to the image.", new Uri(embedBuilder.ImageUrl)), true);
            if (emoji.StartsWith("<a:", StringComparison.Ordinal))
            {
                embedBuilder.AddField("GIF URL", Formatter.MaskedUrl("Link to the GIF.", new Uri(embedBuilder.ImageUrl)), true);
                embedBuilder.ImageUrl = embedBuilder.ImageUrl.Replace(".png", ".gif");
            }

            ImageData? image = await _imageUtilitiesService.GetImageDataAsync(embedBuilder.ImageUrl);
            if (image is null)
            {
                embedBuilder.WithFooter("Failed to get image data.");
                await context.RespondAsync(embedBuilder);
                return;
            }

            if (image.FrameCount != 1)
            {
                // ZWS field
                embedBuilder.AddField("\u200B", "\u200B", true);
                embedBuilder.AddField("Format", image.Format, true);
                embedBuilder.AddField("Frame Count", image.FrameCount.ToString(CultureInfo.InvariantCulture), true);
                // ZWS field
                embedBuilder.AddField("\u200B", "\u200B", true);
            }
            else
            {
                embedBuilder.AddField("Format", image.Format, false);
            }

            embedBuilder.AddField("Resolution", image.Resolution, true);
            embedBuilder.AddField("Dimensions", image.Dimensions, true);
            embedBuilder.AddField("File Size", image.FileSize, true);

            await context.RespondAsync(embedBuilder);
        }

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
            embedBuilder.AddField("Owner", guild.Owner.Mention, true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(guild.CreationTimestamp.UtcDateTime, TimestampFormat.RelativeTime), true);
            // ZWS field
            embedBuilder.AddField("\u200B", "\u200B", true);
            embedBuilder.AddField("Emoji Count", guild.Emojis.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Role Count", guild.Roles.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Sticker Count", guild.Stickers.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Member Count", guild.MemberCount.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Currently Scheduled Events", (guild.ScheduledEvents.Count == 0 ? (await guild.GetEventsAsync(false)).Count : guild.ScheduledEvents.Count).ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Features", string.IsNullOrWhiteSpace(features) ? "None" : features, false);
        }
    }
}
