using System.IO;
using System.Threading.Tasks;
using DSharpPlus;

namespace Tomoe {
    class Program {
        public static Config Config = Tomoe.Config.Init();
        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        private static Logger Logger = new Logger("Main");

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync() {
            Logger.Info("Starting...");
            DiscordClient client = new DiscordClient(new DiscordConfiguration {
                AutoReconnect = true,
                    Token = Config.DiscordApiToken,
                    TokenType = TokenType.Bot,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                    UseRelativeRatelimit = true,
            });

            client.Ready += Tomoe.Utils.Events.OnReady;

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}