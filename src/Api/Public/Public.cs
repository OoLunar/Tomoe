namespace Tomoe.Api
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public partial class Public
    {
        public static Regex RegexArgumentParser { get; private set; } = new("\"([^\"\n]+)\"");
        public static Random Random { get; private set; } = new();
        public static HttpClient HttpClient { get; private set; } = new();
        public static ReadOnlyDictionary<string, MethodInfo> Commands => new(commands);
        public static ReadOnlyDictionary<ulong, int> MemberCount => new(memberCount);
        public static int TotalMemberCount { get; internal set; }
        private const string NotSet = "Not set.";
        private const string LinkToImage = "Link to image.";
        private static readonly Dictionary<string, MethodInfo> commands = new();
        internal static readonly Dictionary<ulong, int> memberCount = new();

        public enum BotInfoField
        {
            GuildCount,
            WebsocketPing,
            MemoryUsed,
            ThreadsOpen
        }

        public static IDictionary<BotInfoField, string> BotInfo(InteractionContext context)
        {
            SortedList<BotInfoField, string> botInfo = new();
            botInfo.Add(BotInfoField.GuildCount, context.Client.Guilds.Count.ToMetric());

            Process currentProcess = Process.GetCurrentProcess();
            botInfo.Add(BotInfoField.MemoryUsed, currentProcess.PrivateMemorySize64.Megabytes().ToString("MB", CultureInfo.InvariantCulture));
            botInfo.Add(BotInfoField.ThreadsOpen, currentProcess.Threads.Count.ToMetric());
            currentProcess.Dispose();

            botInfo.Add(BotInfoField.WebsocketPing, context.Client.Ping.Megabytes().Per(TimeSpan.FromSeconds(1)).Humanize());
            return botInfo;
        }

        public static string Choose(string options)
        {
            // TODO: Have DsharpPlus.InteractivityPlus parse arguments for us.
            CaptureCollection captures = RegexArgumentParser.Match(options).Captures;
            return captures[Random.Next(0, captures.Count)].Value;
        }

        public static DiscordEmbed GuildIcon(InteractionContext context)
        {
            if (context.Guild.IconUrl == null)
            {
                return null;
            }
            else
            {
                DiscordEmbedBuilder discordEmbedBuilder = new()
                {
                    Title = context.Guild.Name + (context.Guild.Name.EndsWith('s') ? "' Guild Icon" : "'s Guild Icon"),
                    ImageUrl = context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"),
                    Color = new DiscordColor("#7b84d1")
                };
                return discordEmbedBuilder;
            }
        }

        public static DiscordEmbed GuildInfo(InteractionContext context)
        {
            string features = string.Join(", ", context.Guild.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
            string bannerUrl = context.Guild.BannerUrl?.Replace(".jpg", ".png?size=1024") ?? NotSet;
            string iconUrl = context.Guild.IconUrl?.Replace(".jpg", ".png?size=1024") ?? NotSet;
            string splashUrl = context.Guild.SplashUrl?.Replace(".jpg", ".png?size=1024") ?? NotSet;
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = context.Guild.Name + " Guild Information",
                Color = new DiscordColor("#7b84d1")
            };
            if (context.Guild.IconUrl != null)
            {
                embedBuilder.WithThumbnail(iconUrl);
            }

            embedBuilder.AddField("AFK Channel", context.Guild.AfkChannel == null ? NotSet : context.Guild.AfkChannel.Mention, true);
            embedBuilder.AddField("AFK Timeout", TimeSpan.FromSeconds(context.Guild.AfkTimeout).Humanize(), true);
            embedBuilder.AddField("Banner Url", context.Guild.BannerUrl == null ? NotSet : Formatter.MaskedUrl(LinkToImage, new(bannerUrl), bannerUrl), true);
            embedBuilder.AddField("Channel Count", context.Guild.Channels.Values.Count(channel => !channel.IsCategory).ToMetric(), true);
            embedBuilder.AddField("Created At", context.Guild.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
            embedBuilder.AddField("Description", string.IsNullOrEmpty(context.Guild.Description) ? NotSet : context.Guild.Description, true);
            embedBuilder.AddField("Emoji Count", context.Guild.Emojis.Count.ToMetric(), true);
            embedBuilder.AddField("Features", string.IsNullOrEmpty(features) ? "None" : features);
            embedBuilder.AddField("Explicit Content Filter", context.Guild.ExplicitContentFilter.Humanize(), true);
            embedBuilder.AddField("Icon url", iconUrl == null ? NotSet : Formatter.MaskedUrl(LinkToImage, new(iconUrl), iconUrl), true);
            embedBuilder.AddField("Id", $"`{context.Guild.Id}`", true);
            embedBuilder.AddField("Max Members", context.Guild.MaxMembers.HasValue ? context.Guild.MaxMembers.Value.ToMetric() : "Unknown", true);
            //embedBuilder.AddField("Member Count", Listeners.GuildDownloadCompleted.MemberCount[context.Guild.Id].ToMetric(), true);
            embedBuilder.AddField("MFA Level", context.Guild.MfaLevel.Humanize(), true);
            embedBuilder.AddField("Name", context.Guild.Name, true);
            embedBuilder.AddField("Owner", context.Guild.Owner.Mention, true);
            embedBuilder.AddField("Preferred Locale", context.Guild.PreferredLocale, true);
            embedBuilder.AddField("Premium Subscription Count", context.Guild.PremiumSubscriptionCount.HasValue ? context.Guild.PremiumSubscriptionCount.Value.ToMetric() : "0", true);
            embedBuilder.AddField("Premium Tier", context.Guild.PremiumTier.Humanize(), true);
            embedBuilder.AddField("Role Count", context.Guild.Roles.Count.ToMetric(), true);
            embedBuilder.AddField("Rules Channel", context.Guild.RulesChannel == null ? NotSet : context.Guild.RulesChannel.Mention, true);
            embedBuilder.AddField("Splash Url", context.Guild.SplashUrl == null ? NotSet : Formatter.MaskedUrl(LinkToImage, new(splashUrl), splashUrl), true);
            embedBuilder.AddField("Vanity Url", string.IsNullOrEmpty(context.Guild.VanityUrlCode) ? NotSet : $"discord.gg/{context.Guild.VanityUrlCode}", true);
            embedBuilder.AddField("Verification Level", context.Guild.VerificationLevel.Humanize(), true);
            embedBuilder.AddField("Voice Region", context.Guild.VoiceRegion.Name, true);

            return embedBuilder;
        }

        public static string GetInvite(ulong botId) => $"https://discord.com/api/oauth2/authorize?client_id={botId}&scope=applications.commands%20bot&permissions=8";

        public static DiscordEmbed GetProfilePicture(DiscordUser discordUser) => new DiscordEmbedBuilder()
        {
            Title = discordUser.Username + (discordUser.Username.EndsWith('s') ? "' Avatar" : "'s Avatar"),
            ImageUrl = discordUser.GetAvatarUrl(ImageFormat.Png, 1024),
            Color = new DiscordColor("#7b84d1")
        };

        public static DiscordEmbed GetProfilePicture(DiscordMember discordMember) => new DiscordEmbedBuilder()
        {
            Title = discordMember.DisplayName + (discordMember.DisplayName.EndsWith('s') ? "' Avatar" : "'s Avatar"),
            ImageUrl = discordMember.GetAvatarUrl(ImageFormat.Png, 1024),
            Color = new DiscordColor("#7b84d1")
        };

        public static async Task<string> Raw(InteractionContext context, string messageString)
        {
            DiscordMessage message;
            DiscordChannel channel = null;
            ulong messageId = 0;
            if (Uri.TryCreate(messageString, UriKind.Absolute, out Uri messageLink))
            {
                if (messageLink.Host != "discord.com" && messageLink.Host != "discordapp.com")
                {
                    //TODO: Make a web request and try escaping the content
                    HttpResponseMessage httpResponseMessage = await HttpClient.GetAsync(messageLink);
                    if (httpResponseMessage.TrailingHeaders.TryGetValues("Content-Type", out IEnumerable<string> contentType))
                    {
                        if (contentType.First() == "text/plain")
                        {
                            string content = await httpResponseMessage.Content.ReadAsStringAsync();
                            return Formatter.Sanitize(content);
                        }
                        return "Error: Content Type is not plain text, will not continue.";
                    }
                }
                else if (messageLink.Segments.Length != 5 || messageLink.Segments[1] != "channels/" || (ulong.TryParse(messageLink.Segments[2].Remove(messageLink.Segments[2].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong guildId) && guildId != context.Guild.Id))
                {
                    return "Error: Message link isn't from this guild!";
                }
                else if (ulong.TryParse(messageLink.Segments[3].Remove(messageLink.Segments[3].Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong channelId))
                {
                    channel = context.Guild.GetChannel(channelId);
                    if (channel == null)
                    {
                        return $"Error: Unknown channel <#{channelId}> ({channelId})";
                    }
                    else if (ulong.TryParse(messageLink.Segments[4], NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageLinkId))
                    {
                        messageId = messageLinkId;
                    }
                }
            }
            else if (ulong.TryParse(messageString, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong messageIdArgs))
            {
                channel = context.Channel;
                messageId = messageIdArgs;
            }
            else
            {
                return $"Error: {messageString} is not a message id or link!";
            }

            try
            {
                message = await channel.GetMessageAsync(messageId);
            }
            catch (NotFoundException)
            {
                return "Error: Message not found! Did you call the command in the correct channel?";
            }
            catch (UnauthorizedException)
            {
                return "Error: I don't have access to that message. Please fix my Discord permissions!";
            }

            return message.Content == string.Empty && message.Embeds.Any() ? Constants.RawEmbed : message.Content;
        }

        public static DiscordEmbed RoleInfo(InteractionContext context, DiscordRole discordRole)
        {
            int totalMemberCount = 0;
            StringBuilder roleMembers = new();
            foreach (DiscordMember member in context.Guild.Members.Values.OrderBy(member => member.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                // TODO: Test if the @everyone role is included in the role list.
                if (member.Roles.Contains(discordRole) || discordRole.Name == "@everyone")
                {
                    totalMemberCount++;
                    // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
                    if (roleMembers.Length < 992)
                    {
                        roleMembers.Append($"{member.Mention} ");
                    }
                }
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Role Info For " + discordRole.Name,
                Color = discordRole.Color.Value == 0x000000 ? new DiscordColor("#7b84d1") : discordRole.Color
            };
            if (context.Guild.IconUrl != null)
            {
                embedBuilder.WithThumbnail(context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"));
            }

            embedBuilder.AddField("Color", discordRole.Color.ToString(), true);
            embedBuilder.AddField("Created At", discordRole.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
            embedBuilder.AddField("Hoisted", discordRole.IsHoisted.ToString(), true);
            embedBuilder.AddField("Is Managed", discordRole.IsManaged.ToString(), true);
            embedBuilder.AddField("Is Mentionable", discordRole.IsMentionable.ToString(), true);
            embedBuilder.AddField("Role Id", discordRole.Id.ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Role Name", discordRole.Name, true);
            embedBuilder.AddField("Role Position", discordRole.Position.ToMetric(), true);
            embedBuilder.AddField("Total Member Count", totalMemberCount.ToMetric(), true);
            embedBuilder.AddField("Permissions", discordRole.Permissions.ToPermissionString());
            embedBuilder.AddField("Members", roleMembers.ToString());

            return embedBuilder;
        }

        public static void SearchCommands(Type type, string commandName = "")
        {
            IEnumerable<Type> nestedTypes = type.GetNestedTypes().Where(type => type?.GetCustomAttribute<SlashCommandGroupAttribute>() != null);
            if (nestedTypes.Any())
            {
                foreach (Type nestedType in nestedTypes)
                {
                    SlashCommandGroupAttribute slashCommandGroupAttribute = nestedType.GetCustomAttribute<SlashCommandGroupAttribute>();
                    commandName += ' ' + slashCommandGroupAttribute.Name;
                    SearchCommands(nestedType, commandName);
                }
            }
            else
            {
                IEnumerable<MethodInfo> localCommands = type.GetMethods().Where(method => method.GetCustomAttribute<SlashCommandAttribute>() != null);
                if (localCommands.Any())
                {
                    foreach (MethodInfo command in localCommands)
                    {
                        SlashCommandAttribute slashCommandAttribute = command.GetCustomAttribute<SlashCommandAttribute>();
                        string subCommand = commandName + ' ' + slashCommandAttribute.Name;
                        commands.TryAdd(subCommand, command);
                    }
                }
            }
        }

        public static string Support() => "https://discord.gg/Bsv7zSFygc";
        public static string RepositoryLink() => Program.Config.RepositoryLink;
    }
}