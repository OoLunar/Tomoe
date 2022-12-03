using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.DSharpPlus.CommandAll;
using OoLunar.DSharpPlus.CommandAll.Parsers;
using OoLunar.Tomoe.Converters.Database;
using OoLunar.Tomoe.Events;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace OoLunar.Tomoe
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();

            // Load the default configuration from the config.json file
            if (File.Exists(Path.Join(Environment.CurrentDirectory, "res", "config.json")))
            {
                configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "res", "config.json"), true, true);
            }

            if (File.Exists(Path.Join(Environment.CurrentDirectory, "res", "config.json.prod")))
            {
                configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "res", "config.json.prod"), true, true);
            }

            // Override the default configuration with the environment variables
            configurationBuilder.AddEnvironmentVariables("DISCORD_BOT_");
            // Then command line args
            configurationBuilder.AddCommandLine(args);

            IConfiguration configuration = configurationBuilder.Build();
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddLogging(logger =>
            {
                string loggingFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

                // Log both to console and the file
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Information)
                .WriteTo.Console(outputTemplate: loggingFormat, theme: new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
                {
                    [ConsoleThemeStyle.Text] = "\x1b[0m",
                    [ConsoleThemeStyle.SecondaryText] = "\x1b[90m",
                    [ConsoleThemeStyle.TertiaryText] = "\x1b[90m",
                    [ConsoleThemeStyle.Invalid] = "\x1b[31m",
                    [ConsoleThemeStyle.Null] = "\x1b[95m",
                    [ConsoleThemeStyle.Name] = "\x1b[93m",
                    [ConsoleThemeStyle.String] = "\x1b[96m",
                    [ConsoleThemeStyle.Number] = "\x1b[95m",
                    [ConsoleThemeStyle.Boolean] = "\x1b[95m",
                    [ConsoleThemeStyle.Scalar] = "\x1b[95m",
                    [ConsoleThemeStyle.LevelVerbose] = "\x1b[34m",
                    [ConsoleThemeStyle.LevelDebug] = "\x1b[90m",
                    [ConsoleThemeStyle.LevelInformation] = "\x1b[36m",
                    [ConsoleThemeStyle.LevelWarning] = "\x1b[33m",
                    [ConsoleThemeStyle.LevelError] = "\x1b[31m",
                    [ConsoleThemeStyle.LevelFatal] = "\x1b[97;91m"
                }))
                .WriteTo.File(
                    $"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: loggingFormat
                );

                // Set Log.Logger for a static reference to the logger
                logger.AddSerilog(loggerConfiguration.CreateLogger());
            });

            // Add the database
            TypeBuilder.AddOrUpdateTypeConverter<UInt64DatabaseConverter>();
            serviceCollection.AddEdgeDB(new EdgeDBConnection()
            {
                Hostname = configuration.GetValue<string>("database:host")!,
                Port = configuration.GetValue<int>("database:port"),
                Database = configuration.GetValue<string>("database:database_name"),
                Username = configuration.GetValue<string>("database:username")!,
                Password = configuration.GetValue<string>("database:password"),
                // Temporarily insecure until I can figure out how to make it secure.
                // I guess it can stay insecure in production since it'll only make connections to localhost
                TLSSecurity = TLSSecurityMode.Insecure
            }, (config) =>
            {
                config.SchemaNamingStrategy = INamingStrategy.SnakeCaseNamingStrategy;
                config.Logger = serviceCollection.BuildServiceProvider().GetRequiredService<ILogger<EdgeDBConnection>>();
            });

            Assembly currentAssembly = typeof(Program).Assembly;
            serviceCollection.AddSingleton(new HttpClient() { DefaultRequestHeaders = { { "User-Agent", $"OoLunar.Tomoe/{currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion} Github" } } });
            serviceCollection.AddSingleton((serviceProvider) =>
            {
                DiscordEventManager eventManager = new(serviceProvider);
                eventManager.GatherEventHandlers(currentAssembly);
                return eventManager;
            });

            // Register the Discord sharded client to the service collection
            serviceCollection.AddSingleton((serviceProvider) =>
            {
                DiscordEventManager eventManager = serviceProvider.GetRequiredService<DiscordEventManager>();
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                DiscordConfiguration discordConfig = new()
                {
                    Token = configuration.GetValue<string>("discord:token"),
                    Intents = DiscordIntents.DirectMessages | DiscordIntents.GuildMembers | DiscordIntents.GuildMessages | DiscordIntents.Guilds | DiscordIntents.MessageContents,
                    LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>()
                };

                DiscordShardedClient shardedClient = new(discordConfig);
                eventManager.RegisterEventHandlers(shardedClient);
                return shardedClient;
            });

            ServiceProvider services = serviceCollection.BuildServiceProvider();
            DiscordShardedClient shardedClient = services.GetRequiredService<DiscordShardedClient>();
            DiscordEventManager eventManager = services.GetRequiredService<DiscordEventManager>();
            IReadOnlyDictionary<int, CommandAllExtension> extensions = await shardedClient.UseCommandAllAsync(new(serviceCollection)
            {
                DebugGuildId = configuration.GetValue<ulong>("discord:debug_guild_id"),
                PrefixParser = new PrefixParser(configuration.GetSection("discord:default_prefixes").Get<string[]>() ?? new[] { ">>" })
            });

            foreach (CommandAllExtension extension in extensions.Values)
            {
                extension.CommandManager.AddCommands(extension, currentAssembly);
                extension.ArgumentConverterManager.AddArgumentConverters(currentAssembly);
                eventManager.RegisterEventHandlers(extension);
            }

            await shardedClient.StartAsync();

            // Wait indefinitely
            await Task.Delay(-1);
        }
    }
}
