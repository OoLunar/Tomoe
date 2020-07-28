using System;
using System.IO;
using System.Reflection;
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
        public static DiscordSocketClient Client = new DiscordSocketClient();
        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        public static Dialog Dialogs = new Dialog();
        public static PreparedStatements PreparedStatements;

        private CommandService commands;
        private IServiceProvider services;

        public static void Main(string[] args) {
            Init init = new Init();
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync() {
            await Client.LoginAsync(TokenType.Bot, Tokens.DocumentElement.SelectSingleNode("discord_api_token").InnerText, true);
            await Client.SetStatusAsync(UserStatus.Idle);
            await Client.SetGameAsync("for enemies to moderate.", null, ActivityType.Watching);
            Client.Log += LoggingFunction;
            services = new ServiceCollection().AddSingleton(Client).AddSingleton<InteractiveService>().BuildServiceProvider();
            commands = new CommandService();
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            Client.MessageReceived += HandleCommandAsync;
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
                await commands.ExecuteAsync(context, argPos, services);
            }
        }
    }
}