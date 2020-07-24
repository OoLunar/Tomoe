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
using Tomoe.Utils;

namespace Tomoe {
    class Program {
        public static XmlDocument tokens = new XmlDocument();
        public static DiscordSocketClient client = new DiscordSocketClient();
        public static string folderPath = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        public static string logPath = Path.Join(folderPath, "log/");
        public static string logFile = Path.Join(logPath, $"{DateTime.Now.ToString("ddd, dd MMM yyyy HH.mm.ss")}.log");
        public static string tokenFile = Path.Join(folderPath, "res/tokens.xml");

        private CommandService commands;
        private IServiceProvider services;

        public static void Main(string[] args) {
            //Try setting up logging.
            if (!FileSystem.CreateFile(logFile)) {
                Console.WriteLine("[Logging] Unable to create the logging file. Everything will be logged to Console.");
            } else {
                /*
                StreamWriter sw = new StreamWriter(logFile, true);
                Console.SetError(sw);
                Console.SetOut(sw);
                sw.AutoFlush = true;
                */
            };
            //Try setting up tokens
            if (!File.Exists(tokenFile)) {
                FileSystem.CreateFile(tokenFile);
                File.WriteAllText(tokenFile, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<config>\n\t<DiscordAPIToken>replace_this_with_bot_token</DiscordAPIToken>\n\t<Postgres host=\"replace_hostname\" database=\"replace_database_name\" username=\"replace_username\" password=\"replace_password\">\n</Postgres>\n</config>");
                Console.WriteLine($"[Credentials] '{tokenFile}' was created, which means that the Discord Bot Token and the PostgreSQL information will need to be filled out. All PostgreSQL information should be alphanumeric.");
                System.Environment.Exit(1);
            }
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync() {
            tokens.Load(tokenFile);
            await client.LoginAsync(TokenType.Bot, tokens.DocumentElement.SelectSingleNode("DiscordAPIToken").InnerText, true);
            await client.SetStatusAsync(UserStatus.Idle);
            await client.SetGameAsync("for enemies to moderate.", null, ActivityType.Watching);
            client.Log += LoggingFunction;
            services = new ServiceCollection().AddSingleton(client).AddSingleton<InteractiveService>().BuildServiceProvider();
            commands = new CommandService();
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            client.MessageReceived += HandleCommandAsync;
            await client.StartAsync();
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
                var context = new SocketCommandContext(client, msg);
                await commands.ExecuteAsync(context, argPos, services);
            }
        }
    }
}