namespace Tomoe
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Tomoe.Utilities.Configs;

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

            logger.Information("Registering commands...");
            Client = new(discordConfiguration);
            SlashCommandsExtension slashCommandsExtension = Client.UseSlashCommands(slashCommandsConfiguration);

            logger.Information("Connecting to Discord...");
            await Client.ConnectAsync();


            Type slashCommandModule = typeof(SlashCommandModule);
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes().Where(type => slashCommandModule.IsAssignableFrom(type) && !type.IsNested))
            {
                // Unregister all guild commands
                foreach (DiscordGuild guild in slashCommandsExtension.Client.Guilds.Values)
                {
                    await guild.BulkOverwriteApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());
                }

                // Shadow
                //slashCommandsExtension.RegisterCommands(type, 719028863530827808);

                // ForSaken Borders
                slashCommandsExtension.RegisterCommands(type, 832354798153236510);

                // Lioness Clubhouse
                //slashCommandsExtension.RegisterCommands(type, 776184288823345191);
            }

            Client.ComponentInteractionCreated += Commands.Listeners.ButtonClicked;
            Client.GuildMemberAdded += Commands.Listeners.PersistentRoles;
            Client.GuildMemberRemoved += Commands.Listeners.PersistentRoles;
            Client.GuildDownloadCompleted += Commands.Listeners.GuildDownloadCompleted;
            Client.GuildAvailable += Commands.Listeners.GuildMemberCache;
            Client.GuildCreated += Commands.Listeners.GuildMemberCache;
            slashCommandsExtension.SlashCommandErrored += Commands.Listeners.CommandErrored;
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
            await Task.Delay(-1);
        }
    }
}