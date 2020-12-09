using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Tomoe.Utils;

namespace Tomoe {
    class Program {
        public const string MissingReason = "**[No reason was provided]**";
        public const string MissingPermissions = "**[Denied: Missing permissions]**";
        public const string NotAGuild = "**[Denied: Guild command]**";
        public const string SelfAction = "**[Denied: Cannot execute on myself]**";
        public const string Hierarchy = "**[Denied: Prevented by hierarchy]**";
        public static Config Config = Tomoe.Config.Init();
        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        public static Database.Database Database = new Database.Database();
        private static Logger _logger = new Logger("Main");
        private static CommandService _commandService;
        private static Logger _dSharpPlusLogger = new Logger("DSharpPlus");

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync() {
            _logger.Info("Starting...");
            using var loggerProvider = new LoggerProvider();
            DiscordClient client = new DiscordClient(new DiscordConfiguration {
                AutoReconnect = true,
                    Token = Config.DiscordApiToken,
                    TokenType = TokenType.Bot,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                    UseRelativeRatelimit = true,
                    MessageCacheSize = 512,
                    LoggerFactory = loggerProvider,
            });

            client.UseInteractivity(new InteractivityConfiguration {
                // default pagination behaviour to just ignore the reactions
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                    // default timeout for other actions to 2 minutes
                    Timeout = TimeSpan.FromMinutes(2)
            });

            client.MessageReactionAdded += Tomoe.Commands.Listeners.ReactionAdded.Main;
            client.Ready += Tomoe.Utils.Events.OnReady;
            new CommandService(Config, client);

            await client.ConnectAsync();
            _logger.Info("Started.");
            await Task.Delay(-1);
        }

        public static async Task<DiscordMessage> SendMessage(CommandContext context, string content, ExtensionMethods.FilteringAction filteringAction = (ExtensionMethods.FilteringAction.CodeBlocksEscape | ExtensionMethods.FilteringAction.AllMentions)) {
            try {
                return await context.RespondAsync($"{context.User.Mention}: {ExtensionMethods.Filter(content, context, filteringAction)}");
            } catch (DSharpPlus.Exceptions.UnauthorizedException) {
                return await (await context.Member.CreateDmChannelAsync()).SendMessageAsync($"Responding to <{context.Message.JumpLink}>: {ExtensionMethods.Filter(content, context, filteringAction)}");
            }
        }

        public static async Task<DiscordMessage> SendMessage(CommandContext context, DiscordEmbed embed) {
            try {
                return await context.RespondAsync($"{context.User.Mention}: ", false, embed);
            } catch (DSharpPlus.Exceptions.UnauthorizedException) {
                return await (await context.Member.CreateDmChannelAsync()).SendMessageAsync($"Responding to <{context.Message.JumpLink}>: ", false, embed);
            }
        }
    }

    public static class ExtensionMethods {
        [Flags]
        public enum FilteringAction {
            CodeBlocksIgnore = 1,
            CodeBlocksEscape = 2,
            CodeBlocksZeroWidthSpace = 4,
            UserMentions = 8,
            RoleMentions = 16,
            AllMentions = 32,
            IgnoreAll = 64,
            FilterAll = 128
        }

        public static string Filter(this string modifyString, CommandContext context, FilteringAction filteringAction = FilteringAction.FilterAll) {
            if (string.IsNullOrEmpty(modifyString)) return null;
            if (filteringAction.HasFlag(FilteringAction.UserMentions) || filteringAction.HasFlag(FilteringAction.FilterAll) || filteringAction.HasFlag(FilteringAction.AllMentions))
                while (Regex.IsMatch(modifyString, @"<@!?(\d+)>", RegexOptions.Multiline)) {
                    // Replaces all mentions with ID's
                    string mentionNickID = Regex.Match(modifyString, @"<@!?(\d+)>", RegexOptions.Multiline).Groups[1].Value;
                    if (!string.IsNullOrEmpty(mentionNickID)) {
                        mentionNickID = $"`<\0@{mentionNickID}>` (User {context.Guild.GetMemberAsync(ulong.Parse(mentionNickID)).GetAwaiter().GetResult().GetCommonName()})";
                        modifyString = Regex.Replace(modifyString, @"<@!?\d+>", mentionNickID);
                    }
                }
            if (filteringAction.HasFlag(FilteringAction.RoleMentions) || filteringAction.HasFlag(FilteringAction.FilterAll) || filteringAction.HasFlag(FilteringAction.AllMentions))
                while (Regex.IsMatch(modifyString, @"<@&(\d+)>", RegexOptions.Multiline) || modifyString.Contains("@everyone") || modifyString.Contains("@here")) {
                    string roleID = Regex.Match(modifyString, @"<@&(\d+)>", RegexOptions.Multiline).Groups[1].Value;
                    if (!string.IsNullOrEmpty(roleID)) {
                        roleID = $"`<\0@\0&{roleID}>` (Role {context.Guild.GetRole(ulong.Parse(roleID)).Name})";
                        modifyString = Regex.Replace(modifyString, @"<@&\d+>", roleID);
                    }
                    modifyString = modifyString.Replace("@everyone", $"`<\0@\0&{context.Guild.Id}>` (Failed everyone attempt)").Replace("@here", $"`<\0@\0&{context.Guild.Id}>` (Failed here attempt)");
                }
            if (filteringAction.HasFlag(FilteringAction.CodeBlocksZeroWidthSpace) || filteringAction.HasFlag(FilteringAction.FilterAll)) return modifyString.Replace("`", "​`​"); // There are zero width spaces before and after the backtick.
            else if (filteringAction.HasFlag(FilteringAction.CodeBlocksEscape)) return modifyString.Replace("\\", "\\\\").Replace("`", "\\`");
            else return modifyString;
        }

        public static string GetCommonName(this DiscordMember guildMember) {
            if (guildMember == null) return null;
            else return guildMember.Nickname != null ? guildMember.Nickname : guildMember.Username;
        }
    }
}