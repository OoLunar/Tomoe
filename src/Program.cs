using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tomoe.Models;
using Tomoe.Utils;

namespace Tomoe
{
    public class Program
    {
        private static DiscordShardedClient DiscordShardedClient { get; set; } = null!;

        public static async Task Main(string[] args)
        {
            FileUtils.CreateDefaultConfig();
            IServiceCollection services = new ServiceCollection();

            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile(Path.Join(FileUtils.GetConfigPath(), "config.json"), true, true);
            configurationBuilder.AddJsonFile(Path.Join(FileUtils.GetConfigPath(), "config.json.prod"), true, true);
            configurationBuilder.AddEnvironmentVariables("TOMOE_");
            configurationBuilder.AddCommandLine(args);
            IConfigurationRoot? configuration = configurationBuilder.Build();
            services.AddSingleton(configuration);

            services.AddLogging(loggingBuilder => // Setup logging with default values specified, in case no configs, env vars or cmd args are provided. Default values still allow for clean error codes, which are somewhat helpful in debugging.
            {
                LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                    .Enrich.WithThreadId()
                    .MinimumLevel.Is(configuration.GetValue("logging:level", LogEventLevel.Information))
                    .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));

                // Allow specific namespace log level overrides, which allows us to hush output from things like the database basic SELECT queries on the Information level.
                foreach (IConfigurationSection logOverride in configuration.GetSection("logging:overrides").GetChildren())
                {
                    loggerConfiguration.MinimumLevel.Override(logOverride.Key, Enum.Parse<LogEventLevel>(logOverride.Value));
                }

                loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd' 'HH'_'mm'_'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: configuration.GetValue("logging:rollingInterval", RollingInterval.Day), outputTemplate: configuration.GetValue("logging:format", "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}"));
                Log.Logger = loggerConfiguration.CreateLogger().ForContext<Program>();

                loggingBuilder.ClearProviders();
                if (!configuration.GetValue("logging:disabled", false))
                {
                    loggingBuilder.AddSerilog(Log.Logger, dispose: true);
                }
            });

            services.AddDbContext<DatabaseContext>(options =>
            {
                NpgsqlConnectionStringBuilder connectionBuilder = new();
                connectionBuilder.ApplicationName = configuration.GetValue("database:applicationName", "Tomoe Discord Bot");
                connectionBuilder.Database = configuration.GetValue("database:databaseName", "tomoe");
                connectionBuilder.Host = configuration.GetValue("database:host", "localhost");
                connectionBuilder.Username = configuration.GetValue("database:username", "tomoe");
                connectionBuilder.Port = configuration.GetValue("database:port", 5432);
                connectionBuilder.Password = configuration.GetValue<string>("database:password");
                connectionBuilder.Timeout = 30;
                options.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure(5));
                options.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);

                DatabaseContext databaseContext = new((DbContextOptions<DatabaseContext>)options.Options);
                databaseContext.Database.EnsureCreated();
            }, ServiceLifetime.Transient);

            CancellationTokenSource cancellationTokenSource = new();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                DiscordShardedClient.StopAsync().GetAwaiter().GetResult();
                cancellationTokenSource.Cancel();
            };
            services.AddSingleton(cancellationTokenSource);
            ServiceProvider? serviceProvider = services.BuildServiceProvider();

            DiscordConfiguration discordConfiguration = new();
            discordConfiguration.LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            discordConfiguration.Token = configuration.GetValue<string>("discord:token");
            discordConfiguration.Intents = DiscordIntents.DirectMessages | DiscordIntents.DirectMessageReactions // Allow text commands to be used in DM's
                | DiscordIntents.GuildBans // Logging
                | DiscordIntents.GuildMembers | DiscordIntents.GuildPresences // Caching
                | DiscordIntents.GuildMessages | DiscordIntents.GuildMessageReactions // Allow text commands to be used
                | DiscordIntents.Guilds; // Logging and config events

            // TODO: Prefix resolver
            CommandsNextConfiguration commandsNextConfiguration = new();
            commandsNextConfiguration.EnableDefaultHelp = false;
            commandsNextConfiguration.Services = serviceProvider;
            commandsNextConfiguration.StringPrefixes = configuration.GetValue("discord:prefixes", new[] { ">>" });

            InteractivityConfiguration interactivityConfiguration = new();
            interactivityConfiguration.AckPaginationButtons = true;
            interactivityConfiguration.ButtonBehavior = ButtonPaginationBehavior.Disable;
            interactivityConfiguration.Timeout = configuration.GetValue("discord:pagination_timeout", TimeSpan.FromMinutes(5));

            DiscordShardedClient = new(discordConfiguration);
            IReadOnlyDictionary<int, CommandsNextExtension>? commandsNextExtensions = await DiscordShardedClient.UseCommandsNextAsync(commandsNextConfiguration);
            MethodInfo registerConverterMethod = typeof(CommandsNextExtension).GetMethod(nameof(CommandsNextExtension.RegisterConverter))!;
            foreach (CommandsNextExtension commandsNextExtension in commandsNextExtensions.Values)
            {
                // Converters
                foreach (Type? converter in typeof(Program).Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IArgumentConverter))).ToList())
                {
                    registerConverterMethod.MakeGenericMethod(converter.GetInterfaces()[0].GenericTypeArguments).Invoke(commandsNextExtension, new object[] { converter.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<object>()) });
                }

                // Commands
                commandsNextExtension.RegisterCommands(typeof(Program).Assembly);

                // TODO: Add an EventListener attribute which specifies which events the class should be registered on.
            }

            await DiscordShardedClient.UseInteractivityAsync(interactivityConfiguration);
            await DiscordShardedClient.StartAsync();

            try
            {
                await Task.Delay(-1, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Log.Information("Shutdown requested! Shutting down...");
            }
        }
    }
}