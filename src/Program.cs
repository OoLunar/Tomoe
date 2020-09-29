using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Tomoe.Utils.Cache;

namespace Tomoe {
    class Program {
        public static Tokens Tokens { get; internal set; }
        public static DiscordSocketClient Client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info, MessageCacheSize = 100 });
        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        public static Tomoe.Utils.Dialog.Root Dialogs = new Tomoe.Utils.Dialog.Root();
        public static PreparedStatements PreparedStatements;

        public static CommandService Commands;
        private IServiceProvider services;

        public static void Main(string[] args) {
            Init init = new Init();
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync() {
            await Client.LoginAsync(TokenType.Bot, Tokens.DiscordApiToken, true);
            await Client.SetStatusAsync(UserStatus.Idle);
            await Client.SetGameAsync("for enemies to moderate.", null, ActivityType.Watching);
            services = new ServiceCollection().AddSingleton(Client).AddSingleton<InteractiveService>().BuildServiceProvider();
            Commands = new CommandService();
            Client.Log += LoggingFunction;
            Commands.AddTypeReader(typeof(Mention), new Mention());
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            Client.MessageReceived += HandleCommandAsync;
            Client.MessageUpdated += Listeners.MessageUpdate;
            Client.MessageDeleted += Listeners.MessageDelete;
            Client.UserJoined += Listeners.AntiRaid;
            await Client.StartAsync();
            Console.WriteLine("[Program] Tomoe has successfully started!");
            await Task.Delay(-1);
        }

        private static Task LoggingFunction(LogMessage msg) {
            Console.WriteLine($"[Logging] {msg.Severity}: {msg.Message} {msg.Exception}");
            return Task.CompletedTask;
        }

        public async Task HandleCommandAsync(SocketMessage m) {
            int argPos = 0;
            if (!(m is SocketUserMessage msg) || msg.Author.IsBot) return;
            if (msg.HasStringPrefix(">>", ref argPos) || msg.HasStringPrefix("tomoe, ", ref argPos) || msg.HasStringPrefix("Tomoe, ", ref argPos) || msg.HasStringPrefix("tomoe ", ref argPos) || msg.HasStringPrefix("Tomoe ", ref argPos) || msg.HasStringPrefix("<@481314095723839508> ", ref argPos)) {
                var context = new SocketCommandContext(Client, msg);
                await Commands.ExecuteAsync(context, argPos, services);
            }
        }
    }

    public class Mention : TypeReader {
        public ulong Id;
        public IUser User;

        public Mention() { }
        public Mention(IUser user) {
            Id = user.Id;
            User = user;
        }

        public Mention(ulong id) {
            Id = id;
            User = Program.Client.Rest.GetUserAsync(id).GetAwaiter().GetResult();
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            string id = Regex.Match(input, @"<@!?(\d+)>", RegexOptions.Multiline).Groups[1].Value;
            Mention mention = new Mention();
            return (ulong.TryParse(id, out mention.Id)) ?
                Task.FromResult(TypeReaderResult.FromSuccess(mention)) :
                Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse Mention."));
        }
    }

    public class Tokens {
        [JsonProperty("database_details")]
        public DatabaseInfo DatabaseDetails;

        [JsonProperty("discord_api_token")]
        public string DiscordApiToken;

        [JsonProperty("log_to_file")]
        public bool LogToFile;

        public class DatabaseInfo {
            [JsonProperty("database")]
            public string Database;

            [JsonProperty("host")]
            public string Host;

            [JsonProperty("password")]
            public string Password;

            [JsonProperty("port")]
            public int Port;

            [JsonProperty("ssl_mode")]
            public Npgsql.SslMode SslMode;

            [JsonProperty("username")]
            public string Username;
        }
    }
}