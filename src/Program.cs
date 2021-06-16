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
            Type slashCommandModule = typeof(SlashCommandModule);
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes().Where(type => slashCommandModule.IsAssignableFrom(type) && !type.IsNested))
            {
                slashCommandsExtension.RegisterCommands(type, 776184288823345191);
            }

            // Until ShardedDiscordClient is fixed
            //foreach (SlashCommandsExtension slashCommandShardExtension in (await Client.UseSlashCommandsAsync(slashCommandsConfiguration)).Values)
            //{
            //    foreach (Type slashCommandClass in Assembly.GetEntryAssembly().GetTypes().Where(type => type?.GetCustomAttribute<SlashCommandAttribute>() != null && !type.IsNested))
            //    {
            //        slashCommandShardExtension.RegisterCommands(slashCommandClass);
            //        slashCommandShardExtension.SlashCommandErrored += Commands.Listeners.CommandErrored.Handler;
            //    }
            //}
            logger.Information("Commands up!");

            Client.GuildMemberAdded += Commands.Listeners.PersistentRoles.Handler;
            Client.GuildMemberRemoved += Commands.Listeners.PersistentRoles.Handler;
            Client.GuildDownloadCompleted += Commands.Listeners.GuildDownloadCompleted.Handler;
            Client.GuildAvailable += Commands.Listeners.GuildMemberCache.Handler;
            slashCommandsExtension.SlashCommandErrored += Commands.Listeners.CommandErrored.Handler;

            logger.Information("Connecting to Discord...");
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}