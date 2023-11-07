using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.ContextChecks;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("info")]
    public sealed partial class InfoCommand
    {
        private static readonly ReadOnlyMemory<char> _slashPrefix = new[] { ',', ' ', '`', '/', '`' };
        private static readonly Dictionary<string, string> UnicodeEmojis = (Dictionary<string, string>)typeof(DiscordEmoji).GetProperty("UnicodeEmojis", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        private readonly ImageUtilitiesService _imageUtilitiesService;
        private readonly DatabaseContext _database;

        public InfoCommand(DatabaseContext database, ImageUtilitiesService imageUtilitiesService)
        {
            _database = database;
            _imageUtilitiesService = imageUtilitiesService;
        }

        [Command("user")]
        public async Task UserInfoAsync(CommandContext context, DiscordUser? user = null)
        {
            user ??= context.User;

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Info about {user.Username}",
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

                GuildMemberModel memberModel = await _database.Members.FirstAsync(databaseMember => member.Id == databaseMember.UserId && member.Guild.Id == databaseMember.GuildId);
                if (memberModel.JoinedAt != member.JoinedAt.UtcDateTime)
                {
                    embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), false);
                    embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
                    embedBuilder.AddField("First joined the Server", Formatter.Timestamp(memberModel.JoinedAt, TimestampFormat.RelativeTime), true);
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

        [Command("bot")]
        public async Task BotInfoAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Bot Info",
                Color = new DiscordColor("#6b73db")
            };

            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();
            embedBuilder.AddField("Heap Memory", GC.GetTotalMemory(false).Bytes().ToString(), true);
            embedBuilder.AddField("Process Memory", currentProcess.WorkingSet64.Bytes().ToString(), true);
            embedBuilder.AddField("Runtime Version", RuntimeInformation.FrameworkDescription, true);

            embedBuilder.AddField("Operating System", $"{Environment.OSVersion} {RuntimeInformation.OSArchitecture.ToString().ToLower(CultureInfo.InvariantCulture)}", true);
            embedBuilder.AddField("Uptime", GetLastCommaRegex().Replace((Process.GetCurrentProcess().StartTime - DateTime.Now).Humanize(3), " and "), true);
            embedBuilder.AddField("Websocket Ping", GetLastCommaRegex().Replace(context.Client.Ping.Milliseconds().Humanize(3), " and "), true);

            embedBuilder.AddField("Guild Count", _database.Guilds.LongCount().ToString("N0"), true);
            embedBuilder.AddField("User Count", _database.Members.LongCount().ToString("N0"), true);

            List<string> prefixes = [];
            prefixes.Add("`>>`");
            prefixes.Add("`/`");
            prefixes.Add(context.Client.CurrentUser.Mention);
            embedBuilder.AddField("Prefixes", string.Join(", ", prefixes), true);
            embedBuilder.AddField("Bot Version", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            embedBuilder.AddField("DSharpPlus Library Version", typeof(DiscordClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);
            embedBuilder.AddField("CommandAll Library Version", typeof(CommandAllExtension).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion, true);

            await context.RespondAsync(embedBuilder);
        }

        [GeneratedRegex(", (?=[^,]*$)", RegexOptions.Compiled)]
        private static partial Regex GetLastCommaRegex();

        [Command("role"), RequireGuild]
        public async Task RoleInfoAsync(CommandContext context, DiscordRole role)
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
                Color = role.Color.Value == 0x000000 ? Optional.FromNoValue<DiscordColor>() : role.Color
            };
            embedBuilder.AddField("Color", role.Color.ToString(), true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(role.CreationTimestamp.UtcDateTime, TimestampFormat.LongDateTime), true);
            embedBuilder.AddField("Hoisted", role.IsHoisted.ToString(), true);
            embedBuilder.AddField("Is Managed", role.IsManaged.ToString(), true);
            embedBuilder.AddField("Is Mentionable", role.IsMentionable.ToString(), true);
            embedBuilder.AddField("Role Id", $"`{role.Id.ToString(CultureInfo.InvariantCulture)}`", true);
            embedBuilder.AddField("Role Name", role.Name, true);
            embedBuilder.AddField("Role Position", role.Position.ToString("N0"), true);
            embedBuilder.AddField("Permissions", role.Permissions == Permissions.None ? "No permissions." : role.Permissions.ToPermissionString() + ".", false);

            List<GuildMemberModel> members = await _database.Members.AsNoTracking().Where(member => member.GuildId == context.Guild!.Id && member.RoleIds.Contains(role.Id) && !member.Flags.HasFlag(MemberState.Absent | MemberState.Banned)).ToListAsync();
            members.Sort((member1, member2) => member1.UserId.CompareTo(member2.UserId));
            embedBuilder.AddField("Member Count", members.Count.ToString("N0", CultureInfo.InvariantCulture), false);
            embedBuilder.AddField("Members", string.Join(", ", members.Select(member => $"<@{member.UserId}>").DefaultIfEmpty("None")), true);

            await context.RespondAsync(embedBuilder);
        }

        [Command("emoji")]
        public async Task EmojiInfoAsync(CommandContext context, string emoji)
        {
            // Grab the emoji name, id, filesize and url.
            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#6b73db")
            };

            if (DiscordEmoji.TryFromUnicode(context.Client, emoji, out DiscordEmoji? discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", UnicodeEmojis.First(x => x.Value == discordEmoji.Name).Key.Replace(":", "\\:"), true);
                embedBuilder.AddField("Unicode", $"\\{discordEmoji.Name}", true);
                embedBuilder.ImageUrl = $"https://raw.githubusercontent.com/twitter/twemoji/master/assets/72x72/{char.ConvertToUtf32(discordEmoji.Name, 0).ToString("X4").ToLower(CultureInfo.InvariantCulture)}.png";
            }
            else if (DiscordEmoji.TryFromName(context.Client, emoji, out discordEmoji))
            {
                embedBuilder.AddField("Emoji Name", discordEmoji.Name, true);
                embedBuilder.AddField("Emoji ID", $"`{discordEmoji.Id.ToString(CultureInfo.InvariantCulture)}`", true);
                embedBuilder.ImageUrl = discordEmoji.Url;
            }
            else
            {
                Match match = GetEmojiRegex().Match(emoji);
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

            ImageData image = await _imageUtilitiesService.GetImageDataAsync(embedBuilder.ImageUrl);
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

        [GeneratedRegex("<a?:(\\w+):(\\d+)>", RegexOptions.Compiled)]
        private static partial Regex GetEmojiRegex();

        [Command("guild"), TextAlias("server")]
        public async Task GuildInfoAsync(CommandContext context, ulong? guildId = null)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#6b73db"),
                Author = new()
                {
                    Name = context.Member?.DisplayName ?? context.User.Username,
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
            embedBuilder.AddField("Member Count", (await _database.Members.CountAsync(member => member.GuildId == guild.Id && !member.Flags.HasFlag(MemberState.Absent | MemberState.Banned))).ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Currently Scheduled Events", (guild.ScheduledEvents.Count == 0 ? (await guild.GetEventsAsync(false)).Count : guild.ScheduledEvents.Count).ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Features", string.IsNullOrWhiteSpace(features) ? "None" : features, false);
        }
    }
}
