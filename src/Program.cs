using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Converters;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Services;
using OoLunar.Tomoe.Utilities;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace OoLunar.Tomoe
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();

            // Load the default configuration from the config.json file
            string configurationFilePath = Path.Join(Environment.CurrentDirectory, "res", "config.json");
            if (File.Exists(configurationFilePath))
            {
                configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "res", "config.json"), true, true);
            }

            // Override the default configuration with the environment variables
            configurationBuilder.AddEnvironmentVariables("DISCORD_BOT_");
            // Then command line args
            configurationBuilder.AddCommandLine(args);

            IConfiguration configuration = configurationBuilder.Build();
            CancellationTokenSource cancellationTokenSource = new(); // TODO: Switch to the `worker` dotnet template so I don't have to manage the cancellation token myself (also handles SIGTERM and alike for me)
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton(cancellationTokenSource); // Adding the source since the token is a struct (the generic requires a reference type to be passed)
            serviceCollection.AddLogging(logger =>
            {
                // Log both to console and the file
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

                // Set Log.Logger for a static reference to the logger
                Log.Logger = loggerConfiguration.CreateLogger();
                logger.AddSerilog(Log.Logger);
            });

            // Add the database
            serviceCollection.AddEdgeDB(new EdgeDBConnection()
            {
                Hostname = configuration.GetValue<string>("database:host"),
                Port = configuration.GetValue<int>("database:port"),
                Database = configuration.GetValue<string>("database:databaseName"),
                Username = configuration.GetValue<string>("database:username"),
                Password = configuration.GetValue<string>("database:password"),
                TLSSecurity = TLSSecurityMode.Insecure // Temporarily insecure until I can figure out how to make it secure. I guess it can stay insecure in production since it'll only make connections to localhost
            }, (config) => config.Logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<EdgeDBClient>());


            // Tracks multiple objects and their changes
            serviceCollection.AddSingleton<DatabaseTracker>();
            // Add the expirable service for reminders and polls (and other things that expire)
            serviceCollection.AddSingleton(typeof(ExpirableService<>));
            // Add the guild model resolver for caching
            serviceCollection.AddSingleton<GuildModelResolverService>();
            // The prefix resolver service is needed since there can be multiple prefixes per guild
            serviceCollection.AddSingleton<DiscordGuildPrefixResolverService>();
            // Automatically register all events and intents through reflection
            serviceCollection.AddSingleton<DiscordEventManager>();
            // Register the Discord sharded client to the service collection
            serviceCollection.AddSingleton(serviceProvider =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                DiscordEventManager eventManager = serviceProvider.GetRequiredService<DiscordEventManager>();
                DiscordConfiguration discordConfig = new()
                {
                    Token = configuration.GetValue<string>("discord:token"),
                    Intents = eventManager.GetIntents() | DiscordIntents.Guilds | DiscordIntents.GuildMessages | DiscordIntents.DirectMessages, // Intents plus CommandsNext required intents
                    LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>()
                };

                DiscordShardedClient shardedClient = new(discordConfig);
                return shardedClient;
            });

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            DiscordShardedClient shardedClient = serviceProvider.GetRequiredService<DiscordShardedClient>();
            DiscordEventManager eventManager = serviceProvider.GetRequiredService<DiscordEventManager>();
            DiscordGuildPrefixResolverService prefixResolverService = serviceProvider.GetRequiredService<DiscordGuildPrefixResolverService>();

            // Subscribe to the events that the sharded client has
            eventManager.Subscribe(shardedClient);
            await shardedClient.UseInteractivityAsync(new()
            {
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.Disable,
                Timeout = configuration.GetValue("discord:pagination_timeout", TimeSpan.FromMinutes(5))
            });

            foreach ((int _, CommandsNextExtension commandsNextExtension) in await shardedClient.UseCommandsNextAsync(new CommandsNextConfiguration()
            {
                Services = serviceProvider,
                PrefixResolver = prefixResolverService.ResolveAsync
            }))
            {
                commandsNextExtension.RegisterCommands(typeof(Program).Assembly);
                commandsNextExtension.RegisterConverter(new ImageFormatConverter());
                // Subscribe to the events that the commands next extension has
                eventManager.Subscribe(commandsNextExtension);
            }

            // Cancel the token when the application is shutting down
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel(false);
            };

            // Start the sharded client and cancel it when CTRL+C is pressed
            await shardedClient.StartAsync();
            cancellationTokenSource.Token.Register(async () =>
            {
                if (shardedClient.ShardClients.Count != 0)
                {
                    await shardedClient.StopAsync();
                }
                Environment.Exit(0);
            });

            // Wait indefinitely
            await Task.Delay(-1);
        }
    }
}
