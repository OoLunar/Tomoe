using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Processors.UserCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Events;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace OoLunar.Tomoe
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile("config.json", true, true);
#if DEBUG
            configurationBuilder.AddJsonFile("config.debug.json", true, true);
#endif
            configurationBuilder.AddEnvironmentVariables("Tomoe__");
            configurationBuilder.AddCommandLine(args);

            IConfiguration configuration = configurationBuilder.Build();
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddLogging(logger =>
            {
                string loggingFormat = configuration.GetValue("Logging:Format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] {SourceContext}: {Message:lj}{NewLine}{Exception}") ?? throw new InvalidOperationException("Logging:Format is null");
                string filename = configuration.GetValue("Logging:Filename", "yyyy'-'MM'-'dd' 'HH'.'mm'.'ss") ?? throw new InvalidOperationException("Logging:Filename is null");

                // Log both to console and the file
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(configuration.GetValue("Logging:Level", LogEventLevel.Debug))
                .WriteTo.Console(
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: loggingFormat,
                    theme: new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
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
                    $"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'.'mm'.'ss", CultureInfo.InvariantCulture)}-.log",
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: loggingFormat,
                    rollingInterval: RollingInterval.Day
                );

                // Allow specific namespace log level overrides, which allows us to hush output from things like the database basic SELECT queries on the Information level.
                foreach (IConfigurationSection logOverride in configuration.GetSection("logging:overrides").GetChildren())
                {
                    if (logOverride.Value is null || !Enum.TryParse(logOverride.Value, out LogEventLevel logEventLevel))
                    {
                        continue;
                    }

                    loggerConfiguration.MinimumLevel.Override(logOverride.Key, logEventLevel);
                }

                logger.AddSerilog(loggerConfiguration.CreateLogger());
            });

            serviceCollection.AddSingleton<DatabaseHandler>();

            Assembly currentAssembly = typeof(Program).Assembly;
            serviceCollection.AddSingleton((serviceProvider) =>
            {
                DiscordEventManager eventManager = new(serviceProvider);
                eventManager.GatherEventHandlers(currentAssembly);
                return eventManager;
            });

            // Register the Discord sharded client to the service collection
            serviceCollection.AddSingleton(async (serviceProvider) =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                DiscordShardedClient shardedClient = new(new DiscordConfiguration()
                {
                    Token = configuration.GetValue<string>("discord:token") ?? throw new InvalidOperationException("Missing Discord token."),
                    Intents = DiscordIntents.All,
                    LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>()
                });

                IReadOnlyDictionary<int, CommandsExtension> commandsExtensions = await shardedClient.UseCommandsAsync(new()
                {
                    DebugGuildId = configuration.GetValue<ulong?>("discord:debug_guild_id", null),
                    ServiceProvider = serviceProvider
                });

                TextCommandProcessor textCommandProcessor = new(new() { PrefixResolver = new DefaultPrefixResolver(configuration.GetValue("discord:prefix", ">>") ?? throw new InvalidOperationException("Missing Discord prefix.")).ResolvePrefixAsync });
                textCommandProcessor.AddConverters(currentAssembly);

                SlashCommandProcessor slashCommandProcessor = new();
                slashCommandProcessor.AddConverters(currentAssembly);

                foreach (CommandsExtension extension in commandsExtensions.Values)
                {
                    await extension.AddProcessorsAsync(
                        textCommandProcessor,
                        slashCommandProcessor,
                        new UserCommandProcessor(),
                        new MessageCommandProcessor()
                    );

                    extension.AddCommands(currentAssembly);
                }

                return shardedClient;
            });

            serviceCollection.AddSingleton(new HttpClient() { DefaultRequestHeaders = { { "User-Agent", $"OoLunar.Tomoe/{currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion} Github" } } });
            serviceCollection.AddSingleton<ImageUtilities>();
            serviceCollection.AddSingleton((serviceProvider) =>
            {
                DiscordEventManager eventManager = new(serviceProvider);
                eventManager.GatherEventHandlers(currentAssembly);
                return eventManager;
            });

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            DiscordShardedClient shardedClient = await serviceProvider.GetRequiredService<Task<DiscordShardedClient>>();
            DiscordEventManager eventManager = serviceProvider.GetRequiredService<DiscordEventManager>();
            serviceProvider.GetRequiredService<DatabaseHandler>(); // Init the db connection
            eventManager.RegisterEventHandlers(shardedClient);
            foreach (CommandsExtension extension in shardedClient.GetCommandsExtensions().Values)
            {
                eventManager.RegisterEventHandlers(extension);
            }

            await shardedClient.StartAsync();

            // Wait indefinitely
            await Task.Delay(-1);
        }
    }
}
