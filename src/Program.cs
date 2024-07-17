using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Processors.UserCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OoLunar.Tomoe.Configuration;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Events;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using SerilogLoggerConfiguration = Serilog.LoggerConfiguration;

namespace OoLunar.Tomoe
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(serviceProvider =>
            {
                ConfigurationBuilder configurationBuilder = new();
                configurationBuilder.Sources.Clear();
                configurationBuilder.AddJsonFile("config.json", true, true);
#if DEBUG
                // If the program is running in debug mode, add the debug config file
                configurationBuilder.AddJsonFile("config.debug.json", true, true);
#endif
                configurationBuilder.AddEnvironmentVariables("TOMOE__");
                configurationBuilder.AddCommandLine(args);

                IConfiguration configuration = configurationBuilder.Build();
                TomoeConfiguration? tomoeConfiguration = configuration.Get<TomoeConfiguration>();
                if (tomoeConfiguration is null)
                {
                    Console.WriteLine("No configuration found! Please modify the config file, set environment variables or pass command line arguments. Exiting...");
                    Environment.Exit(1);
                }

                return tomoeConfiguration;
            });

            serviceCollection.AddLogging(logging =>
            {
                IServiceProvider serviceProvider = logging.Services.BuildServiceProvider();
                TomoeConfiguration tomoeConfiguration = serviceProvider.GetRequiredService<TomoeConfiguration>();
                SerilogLoggerConfiguration serilogLoggerConfiguration = new();
                serilogLoggerConfiguration.MinimumLevel.Is(tomoeConfiguration.Logger.LogLevel);
                serilogLoggerConfiguration.WriteTo.Console(
                    formatProvider: CultureInfo.InvariantCulture,
                    outputTemplate: tomoeConfiguration.Logger.Format,
                    theme: AnsiConsoleTheme.Code
                );

                serilogLoggerConfiguration.WriteTo.File(
                    formatProvider: CultureInfo.InvariantCulture,
                    path: $"{tomoeConfiguration.Logger.Path}/{tomoeConfiguration.Logger.FileName}.log",
                    rollingInterval: tomoeConfiguration.Logger.RollingInterval,
                    outputTemplate: tomoeConfiguration.Logger.Format
                );

                // Sometimes the user/dev needs more or less information about a speific part of the bot
                // so we allow them to override the log level for a specific namespace.
                if (tomoeConfiguration.Logger.Overrides.Count > 0)
                {
                    foreach ((string key, LogEventLevel value) in tomoeConfiguration.Logger.Overrides)
                    {
                        serilogLoggerConfiguration.MinimumLevel.Override(key, value);
                    }
                }

                logging.AddSerilog(serilogLoggerConfiguration.CreateLogger());
            });

            serviceCollection.AddSingleton<DatabaseConnectionManager>();
            serviceCollection.AddSingleton<DatabaseHandler>();
            serviceCollection.AddSingleton(typeof(DatabaseExpirableManager<,>), typeof(DatabaseExpirableManager<,>));
            serviceCollection.AddSingleton(services =>
            {
                TomoeConfiguration tomoeConfiguration = services.GetRequiredService<TomoeConfiguration>();
                return new HttpClient()
                {
                    DefaultRequestHeaders = { { "User-Agent", tomoeConfiguration.HttpUserAgent } }
                };
            });

            // Explicit initialization of the allocation rate tracker to prevent incorrect values.
            serviceCollection.AddSingleton(new AllocationRateTracker());
            serviceCollection.AddSingleton<ImageUtilities>();
            serviceCollection.AddSingleton(serviceProvider =>
            {
                DiscordEventManager eventManager = new(serviceProvider, serviceProvider.GetRequiredService<ILogger<DiscordEventManager>>());
                eventManager.GatherEventHandlers(typeof(Program).Assembly);
                return eventManager;
            });

            serviceCollection.AddSingleton(serviceProvider =>
            {
                TomoeConfiguration tomoeConfiguration = serviceProvider.GetRequiredService<TomoeConfiguration>();
                if (tomoeConfiguration.Discord is null || string.IsNullOrWhiteSpace(tomoeConfiguration.Discord.Token))
                {
                    serviceProvider.GetRequiredService<ILogger<Program>>().LogCritical("Discord token is not set! Exiting...");
                    Environment.Exit(1);
                }

                DiscordClientBuilder clientBuilder = DiscordClientBuilder.CreateDefault(tomoeConfiguration.Discord.Token, DiscordIntents.All, serviceCollection);
                clientBuilder.DisableDefaultLogging();
                return clientBuilder.Build();
            });

            // Almost start the program
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            DatabaseHandler databaseHandler = serviceProvider.GetRequiredService<DatabaseHandler>();
            TomoeConfiguration tomoeConfiguration = serviceProvider.GetRequiredService<TomoeConfiguration>();
            DiscordClient discordClient = serviceProvider.GetRequiredService<DiscordClient>();
            DiscordEventManager eventManager = serviceProvider.GetRequiredService<DiscordEventManager>();
            Assembly currentAssembly = typeof(Program).Assembly;

            // Connect to the database
            try
            {
                await databaseHandler.InitializeAsync();
            }
            catch (NpgsqlException error)
            {
                serviceProvider.GetRequiredService<ILogger<Program>>().LogError(error, "Failed to connect to the database - assume broken functionality.");
            }

            // Register extensions here since these involve asynchronous operations
            CommandsExtension commandsExtension = discordClient.UseCommands(new CommandsConfiguration()
            {
                DebugGuildId = tomoeConfiguration.Discord.GuildId,
                UseDefaultCommandErrorHandler = false,
                RegisterDefaultCommandProcessors = false
            });

            // Add all commands by scanning the current assembly
            commandsExtension.AddCommands(currentAssembly);

            // Enable each command type specified by the user
            List<ICommandProcessor> processors = [];
            foreach (string processor in tomoeConfiguration.Discord.Processors)
            {
                if (processor.Equals("text", StringComparison.OrdinalIgnoreCase))
                {
                    TextCommandProcessor textCommandProcessor = new(new()
                    {
                        PrefixResolver = new DefaultPrefixResolver(true, tomoeConfiguration.Discord.Prefix ?? throw new InvalidOperationException("Missing Discord prefix.")).ResolvePrefixAsync
                    });

                    textCommandProcessor.AddConverters(currentAssembly);
                    processors.Add(textCommandProcessor);
                }
                else if (processor.Equals("slash", StringComparison.OrdinalIgnoreCase))
                {
                    SlashCommandProcessor slashCommandProcessor = new();
                    slashCommandProcessor.AddConverters(currentAssembly);
                    processors.Add(slashCommandProcessor);
                }
                else if (processor.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    processors.Add(new UserCommandProcessor());
                }
                else if (processor.Equals("message", StringComparison.OrdinalIgnoreCase))
                {
                    processors.Add(new MessageCommandProcessor());
                }
            }

            await commandsExtension.AddProcessorsAsync(processors);
            eventManager.RegisterEventHandlers(commandsExtension);

            // Register event handlers for the Discord Client itself
            eventManager.RegisterEventHandlers(discordClient);

            // Connect the bot to the Discord gateway.
            await discordClient.ConnectAsync();

            // Wait indefinitely
            await Task.Delay(-1);
        }
    }
}
