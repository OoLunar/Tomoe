namespace Tomoe
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Tomoe.Utils.Configs;

    public class Program
    {
        public static DiscordClient Client { get; private set; }
        public static Config Config { get; private set; }
        public static ServiceProvider ServiceProvider { get; private set; }

        public static void Main() => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            ServiceCollection services = new();
            Config = await Config.Load();
            Config.Logger.Load(services);
            await Config.Database.Load(services);
            ServiceProvider = services.BuildServiceProvider();
            Serilog.ILogger logger = Log.Logger.ForContext<Program>();

            DiscordConfiguration discordConfiguration = new()
            {
                Token = Config.DiscordApiToken,
                LoggerFactory = ServiceProvider.GetService<ILoggerFactory>()
            };

            SlashCommandsConfiguration slashCommandsConfiguration = new()
            {
                Services = ServiceProvider
            };

            logger.Information("Registering commands...");
            Client = new(discordConfiguration);
            SlashCommandsExtension slashCommandsExtension = Client.UseSlashCommands(slashCommandsConfiguration);
            MethodInfo registerCommandMethod = slashCommandsExtension.GetType().GetMethod(nameof(slashCommandsExtension.RegisterCommands));
            foreach (Type someClass in Assembly.GetEntryAssembly().GetTypes().Where(type => type?.GetCustomAttribute<SlashCommandAttribute>() != null && !type.IsNested))
            {
                registerCommandMethod.MakeGenericMethod(new[] { someClass.GetType() }).Invoke(slashCommandsExtension, null);
            }
            logger.Information("Commands up!");

            logger.Information("Connecting to Discord...");
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}