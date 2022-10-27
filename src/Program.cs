using System;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Tomoe.Utilities.Configs;

namespace Tomoe
{
    public class Program
    {
        public static DiscordShardedClient Client { get; private set; }
        public static Config Config { get; private set; }
        public static ServiceProvider ServiceProvider { get; private set; }

        public static void Main() => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            ServiceCollection services = new();
            Config = await Config.LoadAsync();
            Config.Logger.Load(services);
            await Config.Database.LoadAsync(services);
            ServiceProvider = services.BuildServiceProvider();
            Serilog.ILogger logger = Log.Logger.ForContext<Program>();

            DiscordConfiguration discordConfiguration = new()
            {
                Intents = DiscordIntents.GuildMembers // Required for persistent roles
                    | DiscordIntents.Guilds // Required for channel permissions and member cache
                    | DiscordIntents.GuildPresences // Required for member cache
                    | DiscordIntents.GuildMessages // Required for automod
                    | DiscordIntents.GuildMessageReactions, // Required for reaction roles
                Token = Config.DiscordApiToken,
                LoggerFactory = ServiceProvider.GetService<ILoggerFactory>()
            };

            SlashCommandsConfiguration slashCommandsConfiguration = new()
            {
                Services = ServiceProvider
            };

            Client = new(discordConfiguration);
            Client.MessageCreated += Commands.Listeners.AutoReactionsAsync;
            Client.ComponentInteractionCreated += Commands.Listeners.ButtonClickedAsync;
            Client.GuildDownloadCompleted += Commands.Listeners.GuildDownloadCompletedAsync;
            Client.GuildAvailable += Commands.Listeners.GuildMemberCacheAsync;
            Client.GuildCreated += Commands.Listeners.GuildMemberCacheAsync;
            Client.GuildMemberAdded += Commands.Listeners.PersistentRolesAsync;
            Client.GuildMemberRemoved += Commands.Listeners.PersistentRolesAsync;

            logger.Information("Connecting to Discord...");
            await Client.StartAsync();


            logger.Information("Registering commands...");
            foreach (SlashCommandsExtension slashCommandShardExtension in (await Client.UseSlashCommandsAsync(slashCommandsConfiguration)).Values)
            {
#if DEBUG
                slashCommandShardExtension.RegisterCommands(typeof(Commands.Moderation), Config.DiscordDebugGuildId);
                slashCommandShardExtension.RegisterCommands(typeof(Commands.Public), Config.DiscordDebugGuildId);
#else
                slashCommandShardExtension.RegisterCommands(typeof(Commands.Moderation));
                slashCommandShardExtension.RegisterCommands(typeof(Commands.Public));
#endif
                slashCommandShardExtension.SlashCommandErrored += Commands.Listeners.CommandErroredAsync;
                slashCommandShardExtension.SlashCommandExecuted += Commands.Listeners.CommandExecutedAsync;
            }
            logger.Information("Commands up!");

            Timer timer = new()
            {
                AutoReset = true,
                Interval = TimeSpan.FromMinutes(1).TotalMilliseconds
            };
            timer.Elapsed += Commands.Moderation.TempRoleEventAsync;
            timer.Start();
            logger.Information("Started temporary roles event.");
            await Task.Delay(-1);
        }
    }
}
