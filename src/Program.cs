using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
using Tomoe.Attributes;
using Tomoe.Commands.Common;
using Tomoe.Commands.Moderation;
using Tomoe.Models;
using Tomoe.Utils;

namespace Tomoe
{
    public class Program
    {
        internal static Dictionary<ulong, int> Guilds = new();
        internal static DiscordShardedClient DiscordShardedClient { get; set; } = null!;
        internal static ServiceProvider ServiceProvider { get; set; } = null!;
        internal static bool BotReady { get; set; }

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
                NpgsqlConnectionStringBuilder connectionBuilder = new()
                {
                    ApplicationName = configuration.GetValue("database:applicationName", "Tomoe Discord Bot"),
                    Database = configuration.GetValue("database:databaseName", "tomoe"),
                    Host = configuration.GetValue("database:host", "localhost"),
                    Username = configuration.GetValue("database:username", "tomoe"),
                    Port = configuration.GetValue("database:port", 5432),
                    Password = configuration.GetValue<string>("database:password"),
                    Timeout = 30
                };
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

            services.AddSingleton(serviceProvider =>
            {
                DatabaseList<PollModel, Guid> pollModelList = new(services.BuildServiceProvider());
                pollModelList.ItemExpired += Poll.VoteExpiredAsync;
                return pollModelList;
            });

            services.AddSingleton(serviceProvider =>
            {
                DatabaseList<TempRoleModel, Guid> pollModelList = new(services.BuildServiceProvider());
                pollModelList.ItemExpired += TempRole.RoleExpiredAsync;
                return pollModelList;
            });

            services.AddSingleton(serviceProvider =>
            {
                DatabaseList<ReminderModel, int> reminderList = new(services.BuildServiceProvider());
                //reminderList.ItemExpired += TempRole.RoleExpiredAsync;
                return reminderList;
            });

            ServiceProvider = services.BuildServiceProvider();

            DiscordConfiguration discordConfiguration = new()
            {
                LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>(),
                Token = configuration.GetValue<string>("discord:token"),
                Intents = DiscordIntents.DirectMessages | DiscordIntents.DirectMessageReactions // Allow text commands to be used in DM's
                          | DiscordIntents.GuildBans // Logging
                          | DiscordIntents.GuildMembers | DiscordIntents.GuildPresences // Caching
                          | DiscordIntents.GuildMessages | DiscordIntents.GuildMessageReactions // Allow text commands to be used
                          | DiscordIntents.Guilds // Logging and config events
            };

            // TODO: Prefix resolver
            CommandsNextConfiguration commandsNextConfiguration = new()
            {
                EnableDefaultHelp = false,
                Services = ServiceProvider,
                StringPrefixes = configuration.GetValue("discord:prefixes", new[] { ">>" })
            };

            InteractivityConfiguration interactivityConfiguration = new()
            {
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.Disable,
                Timeout = configuration.GetValue("discord:pagination_timeout", TimeSpan.FromMinutes(5))
            };

            DiscordShardedClient = new(discordConfiguration);
            IReadOnlyDictionary<int, CommandsNextExtension>? commandsNextExtensions = await DiscordShardedClient.UseCommandsNextAsync(commandsNextConfiguration);
            MethodInfo registerConverterMethod = typeof(CommandsNextExtension).GetMethod(nameof(CommandsNextExtension.RegisterConverter))!;
            MethodInfo registerUserFriendlyNameMethod = typeof(CommandsNextExtension).GetMethod(nameof(CommandsNextExtension.RegisterUserFriendlyTypeName))!;
            foreach (CommandsNextExtension commandsNextExtension in commandsNextExtensions.Values)
            {
                // Converters
                foreach (Type? converter in typeof(Program).Assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IArgumentConverter))).ToList())
                {
                    registerConverterMethod.MakeGenericMethod(converter.GetInterfaces()[0].GenericTypeArguments).Invoke(commandsNextExtension, new object[] { converter.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<object>()) });
                    registerUserFriendlyNameMethod.MakeGenericMethod(converter.GetInterfaces()[0].GenericTypeArguments).Invoke(commandsNextExtension, new object[] { converter.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? throw new InvalidOperationException($"Converter {converter.FullName} is missing a user friendly type name.") });
                }

                // Commands
                commandsNextExtension.RegisterCommands(typeof(Program).Assembly);
            }

            EventInfo[] dSharpPlusEvents = typeof(DiscordShardedClient).GetEvents();
            foreach (Type type in typeof(Program).Assembly.GetTypes().Where(t => t.GetMethods().Any(method => method.GetCustomAttribute<SubscribeToEventAttribute>() != null)))
            {
                foreach (MethodInfo methodInfo in type.GetMethods())
                {
                    SubscribeToEventAttribute? subscribeToEventAttribute = methodInfo.GetCustomAttribute<SubscribeToEventAttribute>();
                    if (subscribeToEventAttribute != null)
                    {
                        EventInfo? discordEvent = dSharpPlusEvents.FirstOrDefault(e => e.Name == subscribeToEventAttribute.EventName) ?? throw new CustomAttributeFormatException($"Event {subscribeToEventAttribute.EventName} not found.");
                        discordEvent.AddEventHandler(DiscordShardedClient, Delegate.CreateDelegate(discordEvent.EventHandlerType!, type, methodInfo.Name));
                    }
                }
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
