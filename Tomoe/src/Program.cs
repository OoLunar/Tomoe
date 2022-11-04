using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Tomoe.Commands;
using Tomoe.Commands.Moderation;
using Tomoe.Utilities.Configs;

namespace Tomoe
{
    public class Program
    {
        internal static Config Config { get; private set; } = null!;
        internal static DiscordShardedClient Client { get; private set; } = null!;
        internal static ServiceProvider ServiceProvider { get; private set; } = null!;
        internal static readonly Dictionary<ulong, int> TotalMemberCount = new();

        public static async Task Main()
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
                Services = ServiceProvider,
                ParamNamingStrategy = ParamNamingStrategy.Underscored
            };

            Client = new(discordConfiguration);
            Client.MessageCreated += AutoReactionListener.AutoReactionsAsync;
            Client.ComponentInteractionCreated += ButtonClickedListener.ButtonClickedAsync;
            Client.GuildDownloadCompleted += GuildDownloadCompletedListener.GuildDownloadCompletedAsync;
            Client.GuildAvailable += GuildMemberCacheListener.GuildMemberCacheAsync;
            Client.GuildCreated += GuildMemberCacheListener.GuildMemberCacheAsync;
            Client.GuildMemberAdded += PersistentRolesListener.PersistentRolesAsync;
            Client.GuildMemberRemoved += PersistentRolesListener.PersistentRolesAsync;

            logger.Information("Connecting to Discord...");
            await Client.StartAsync();


            logger.Information("Registering commands...");
            foreach (SlashCommandsExtension slashCommandShardExtension in (await Client.UseSlashCommandsAsync(slashCommandsConfiguration)).Values)
            {
#if DEBUG
                slashCommandShardExtension.RegisterCommands(Assembly.GetExecutingAssembly(), Config.DiscordDebugGuildId);
#else
                slashCommandShardExtension.RegisterCommands(Assembly.GetExecutingAssembly(), 0);
                slashCommandShardExtension.SlashCommandErrored += CommandErroredListener.CommandErroredAsync;
#endif
                slashCommandShardExtension.SlashCommandExecuted += CommandExecutedListener.CommandExecutedAsync;
            }
            logger.Information("Commands up!");

            Timer timer = new()
            {
                AutoReset = true,
                Interval = TimeSpan.FromMinutes(1).TotalMilliseconds
            };
            timer.Elapsed += TempRoleCommand.TempRoleEventAsync;
            timer.Start();
            logger.Information("Started temporary roles event.");
            await Task.Delay(-1);
        }
    }
}
