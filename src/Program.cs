using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Tomoe.Utils;

namespace Tomoe {
    class Program {
        public const string MissingReason = "**[No reason was provided]**";
        public const string MissingPermissions = "**[Denied: Missing Permissions]**";
        public const string NotAGuild = "**[Denied: Guild command]**";
        public const string SelfAction = "**[Denied: Cannot execute on myself]**";
        public static Config Config = Tomoe.Config.Init();
        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        private static Logger _logger = new Logger("Main");
        private static CommandService _commandService;
        private static Logger dSharpPlusLogger = new Logger("DSharpPlus");

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync() {
            _logger.Info("Starting...");
            using var temp = new LoggerProvider();
            DiscordClient client = new DiscordClient(new DiscordConfiguration {
                AutoReconnect = true,
                    Token = Config.DiscordApiToken,
                    TokenType = TokenType.Bot,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                    UseRelativeRatelimit = true,
                    MessageCacheSize = 512,
                    LoggerFactory = temp
            });

            client.Ready += Tomoe.Utils.Events.OnReady;
            new CommandService(Config, client);

            await client.ConnectAsync();
            _logger.Info("Started.");
            await Task.Delay(-1);
        }

        public static async Task SendMessage(CommandContext context, dynamic content) {
            try {
                await context.RespondAsync(content.GetType() == typeof(string) ? $"{context.User.Mention}: {content}" : null, false, content.GetType() == typeof(DiscordEmbed) ? content : null);
            } catch (DSharpPlus.Exceptions.UnauthorizedException) {
                await (await context.Member.CreateDmChannelAsync()).SendMessageAsync($"Responding to <{context.Message.JumpLink}>:", false, content);
            }
        }
    }

    public static class ExtensionMethods {
        public static string Filter(this string modifyString, CommandContext context) {
            if (string.IsNullOrEmpty(modifyString)) return null;
            while (Regex.IsMatch(modifyString, @"<@&(\d+)>", RegexOptions.Multiline) || Regex.IsMatch(modifyString, @"<@!?(\d+)>", RegexOptions.Multiline) || modifyString.Contains("@everyone") || modifyString.Contains("@here")) {
                // Replaces all mentions with ID's
                string mentionNickID = Regex.Match(modifyString, @"<@!?(\d+)>", RegexOptions.Multiline).Groups[1].Value;
                mentionNickID = $"`{mentionNickID}` (User {context.Guild.GetMemberAsync(ulong.Parse(mentionNickID)).GetAwaiter().GetResult().GetCommonName()})";
                modifyString = Regex.Replace(modifyString, @"<@!?\d+>", mentionNickID);
                string roleID = Regex.Match(modifyString, @"<@&(\d+)>", RegexOptions.Multiline).Groups[1].Value;
                roleID = $"`{roleID}` (Role {context.Guild.GetRole(ulong.Parse(roleID)).Name})";
                modifyString = Regex.Replace(modifyString, @"<@&\d+>", roleID);
                modifyString = modifyString.Replace("@everyone", $"`{context.Guild.Id}` (Failed everyone attempt)").Replace("@here", $"`{context.Guild.Id}` (Failed here attempt)");
            }
            // There are zero width spaces before and after the backtick.
            return modifyString.Replace("`", "​`​");
        }

        public static string GetCommonName(this DiscordMember guildMember) {
            if (guildMember == null) return null;
            else return guildMember.Nickname != null ? guildMember.Nickname : guildMember.Username;
        }
    }
}