using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Converters;
using OoLunar.Tomoe.Services;
using OoLunar.Tomoe.Utilities;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace OoLunar.Tomoe
{
    public sealed class Program
    {
        private static IConfiguration Configuration { get; set; } = null!;

        public static async Task Main(string[] args)
        {
            IConfiguration? configuration = LoadConfiguration(args);
            if (configuration == null)
            {
                Console.WriteLine("Failed to load configuration due to unknown errors.");
                Environment.Exit(1); // Respect the Linux users!
                return; // Shutup Roslyn. configuration isn't null.
            }

            CancellationTokenSource cancellationTokenSource = new();
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton(cancellationTokenSource);
            serviceCollection.AddLogging(logger =>
            {
                LoggerConfiguration loggerConfiguration = new();
                loggerConfiguration.MinimumLevel.Is(configuration.GetValue("logging:level", LogEventLevel.Information));
                loggerConfiguration.Enrich.WithThreadId();
                loggerConfiguration.WriteTo.Console(
                    theme: LoggerTheme.Lunar,
                    outputTemplate: configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                );
                loggerConfiguration.WriteTo.File(
                    $"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log",
                    rollingInterval: configuration.GetValue("logging:rollingInterval", RollingInterval.Day),
                    outputTemplate: configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
                );

                Log.Logger = loggerConfiguration.CreateLogger();
                logger.AddSerilog(Log.Logger);
            });

            serviceCollection.AddEdgeDB(new EdgeDBConnection()
            {
                Hostname = configuration.GetValue<string>("database:host"),
                Port = configuration.GetValue<int>("database:port"),
                Database = configuration.GetValue<string>("database:databaseName"),
                Username = configuration.GetValue<string>("database:username"),
                Password = configuration.GetValue<string>("database:password"),
                TLSSecurity = TLSSecurityMode.Insecure
            }, (config) => config.Logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<EdgeDBClient>());

            serviceCollection.AddScoped<DiscordGuildPrefixResolverService>();
            serviceCollection.AddSingleton(serviceProvider => new DiscordEventManager(serviceProvider));
            serviceCollection.AddSingleton(serviceProvider =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                DiscordEventManager eventManager = serviceProvider.GetRequiredService<DiscordEventManager>();

                DiscordConfiguration discordConfig = new()
                {
                    MinimumLogLevel = configuration.GetValue<LogLevel>("discord:logLevel"),
                    Token = configuration.GetValue<string>("discord:token"),
                    Intents = eventManager.GetIntents() | DiscordIntents.Guilds | DiscordIntents.GuildMessages | DiscordIntents.DirectMessages,
                    LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>()
                };

                DiscordShardedClient shardedClient = new(discordConfig);
                return shardedClient;
            });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            EdgeDBClient edgeDBClient = serviceProvider.GetRequiredService<EdgeDBClient>();
            DiscordShardedClient shardedClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
            DiscordEventManager eventManager = serviceProvider.GetRequiredService<DiscordEventManager>();
            DiscordGuildPrefixResolverService prefixResolverService = serviceProvider.GetRequiredService<DiscordGuildPrefixResolverService>();

            eventManager.Subscribe(shardedClient);
            foreach ((int _, CommandsNextExtension commandsNextExtension) in await shardedClient.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                Services = serviceProvider,
                PrefixResolver = prefixResolverService.ResolveAsync
            }))
            {
                commandsNextExtension.RegisterCommands(typeof(Program).Assembly);
                commandsNextExtension.RegisterConverter(new ImageFormatConverter());
                eventManager.Subscribe(commandsNextExtension);
            }

            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel(false);
                if (shardedClient.ShardClients.Count != 0)
                {
                    await shardedClient.StopAsync();
                }
            };

            await shardedClient.StartAsync();
            await Task.Delay(-1);
        }

        internal static IConfiguration? LoadConfiguration(string[] args)
        {
            // Prevent parsing multiple times when the arguments aren't changed.
            if (args.Length == 0 && Configuration != null)
            {
                return Configuration;
            }

            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();

            string configurationFilePath = Path.Join(Environment.CurrentDirectory, "res", "config.json");
            if (File.Exists(configurationFilePath))
            {
                configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "res", "config.json"), true, true);
            }

            configurationBuilder.AddEnvironmentVariables("DISCORD_BOT_");
            configurationBuilder.AddCommandLine(args);

            Configuration = configurationBuilder.Build();
            return Configuration;
        }
    }
}
