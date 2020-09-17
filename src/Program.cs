using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Utils.Cache;

namespace Tomoe {
    class Program {
        public static XmlDocument Tokens = new XmlDocument();
        public static DiscordSocketClient Client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info, MessageCacheSize = 100 });
        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        public static Tomoe.Utils.Dialog Dialogs = new Tomoe.Utils.Dialog();
        public static PreparedStatements PreparedStatements;

        public static CommandService Commands;
        private IServiceProvider services;

        public static void Main(string[] args) {
            Init init = new Init();
            Tomoe.Utils.DialogContext.GetActionType(Tomoe.Utils.DialogContext.Action.Ban);
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync() {
            await Client.LoginAsync(TokenType.Bot, Tokens.DocumentElement.SelectSingleNode("discord_api_token").InnerText, true);
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
            Console.WriteLine(msg);
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
        public Mention() { }
        public Mention(ulong id) => Id = id;

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            string id = Regex.Match(input, @"<@!?(\d+)>", RegexOptions.Multiline).Groups[1].Value;
            Mention mention = new Mention();
            return (ulong.TryParse(id, out mention.Id)) ?
                Task.FromResult(TypeReaderResult.FromSuccess(mention)) :
                Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse Mention."));
        }

    }
}