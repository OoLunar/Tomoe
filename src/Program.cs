using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;
using Tomoe.Commands.Listeners;
using Tomoe.Db;
using Tomoe.Utils;

namespace Tomoe
{
	public class Program
	{
		public static DiscordShardedClient Client { get; private set; }
		public static Utils.Configs.Config Config { get; private set; }
		public static IServiceProvider ServiceProvider { get; private set; }

		public static void Main() => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		public static async Task MainAsync()
		{
			// Setup Config
			// Look for Environment variables for Docker. If the variable is set, but doesn't exist, assume it was improper configuration and exit.
			string tokenFile = Environment.GetEnvironmentVariable("CONFIG_FILE");
			if (tokenFile != null && !File.Exists(tokenFile))
			{
				Console.WriteLine($"The config file \"{tokenFile}\" does not exist. Consider removed the $CONFIG_FILE environment variable or making sure the file exists.");
				Environment.Exit(1);
			}
			else if (File.Exists("res/config.jsonc.prod"))
			{
				// Look for production file first. Contributers are expected not to fill out res/config.jsonc, but res/config.jsonc.prod instead.
				tokenFile = "res/config.jsonc.prod";
			}
			else if (File.Exists("res/config.jsonc"))
			{
				tokenFile = "res/config.jsonc";
			}
			else
			{
				// No config file could be found. Download it for them and inform them of the issue.
				WebClient webClient = new();
				webClient.DownloadFile("https://raw.githubusercontent.com/OoLunar/Tomoe/master/res/config.jsonc", "res/config.jsonc");
				Console.WriteLine("The config file was downloaded. Please go fill out \"res/config.jsonc\". It is recommended to use \"res/config.jsonc.prod\" if you intend on contributing to Tomoe.");
				Environment.Exit(1);
			}
			// Prefer JsonSerializer.DeserializeAsync over JsonSerializer.Deserialize due to being able to send the stream directly.
			Config = await JsonSerializer.DeserializeAsync<Utils.Configs.Config>(File.OpenRead(tokenFile), new() { IncludeFields = true, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true });

			// Setup Logger
			// Follow the Config.Logger.ShowId option.
			string outputTemplate = Config.Logger.ShowId ? "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}" : "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
			LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
				.Enrich.WithThreadId()
				.MinimumLevel.Is(Config.Logger.Tomoe)
				// Per library settings.
				.MinimumLevel.Override("DSharpPlus", Config.Logger.Discord)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore", Config.Logger.Database)
				// Use custom theme because the default one stinks
				.WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: outputTemplate);
			if (Config.Logger.SaveToFile) _ = loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToLocalTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate);
			Log.Logger = loggerConfiguration.CreateLogger();

			// Setup services
			ServiceCollection services = new();
			// Database has scoped lifetime by default
			_ = services.AddDbContext<Database>(options =>
			{
				NpgsqlConnectionStringBuilder connectionBuilder = new();
				connectionBuilder.ApplicationName = Config.Database.ApplicationName;
				connectionBuilder.Database = Config.Database.DatabaseName;
				connectionBuilder.Host = Config.Database.Host;
				connectionBuilder.Password = Config.Database.Password;
				connectionBuilder.Username = Config.Database.Username;
				connectionBuilder.Port = Config.Database.Port;
				_ = options.UseNpgsql(connectionBuilder.ToString(), options =>
				{
					_ = options.EnableRetryOnFailure();
				});
				_ = options.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
				_ = options.EnableSensitiveDataLogging();
				_ = options.EnableDetailedErrors();
				_ = options.UseLoggerFactory(ServiceProvider.GetService<ILoggerFactory>());
			}, ServiceLifetime.Transient);
			_ = services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger));
			ServiceProvider = services.BuildServiceProvider();
			_ = await ServiceProvider.GetService<Database>().Database.EnsureCreatedAsync();

			// Setup Discord
			DiscordConfiguration discordConfiguration = new()
			{
				AutoReconnect = true,
				Token = Config.DiscordApiToken,
				TokenType = TokenType.Bot,
				UseRelativeRatelimit = true,
				MessageCacheSize = 512,
				LoggerFactory = ServiceProvider.GetService<ILoggerFactory>(),
				Intents = DiscordIntents.All
			};

			// Setup event listeners
			Client = new(discordConfiguration);

			Client.ChannelCreated += ChannelCreated.Handler;
			Client.GuildAvailable += GuildAvailable.Handler;
			Client.GuildCreated += GuildCreated.Handler;
			Client.GuildDownloadCompleted += GuildDownloadCompleted.Handler;
			Client.GuildMemberAdded += GuildMemberAdded.Handler;
			Client.GuildMemberRemoved += GuildMemberRemoved.Handler;
			Client.GuildMemberUpdated += GuildMemberUpdated.Handler;
			Client.MessageCreated += AutoReactionListener.Handler;
			Client.MessageCreated += CommandHandler.Handler;
			Client.MessageCreated += MessageRecieved.Handler;
			Client.MessageReactionAdded += ReactionAdded.Handler;
			Client.MessageReactionAdded += ReactionRoleAdded.Handler;
			Client.MessageReactionRemoved += ReactionRoleRemoved.Handler;
			Console.CancelKeyPress += Quit.ConsoleShutdown;
			await CommandService.Launch(Client, ServiceProvider);
			await Client.StartAsync();
			await Task.Delay(-1);
		}

		/// <summary>
		/// Used to sucessfully filter out unwanted messages and to always reply to the command. The `content` or `embed` can be null, but one must have a value.
		/// </summary>
		/// <param name="context">CommandContext, used for the user Id, the channel Id, the message Id and to get the member object.</param>
		/// <param name="content">The message to send</param>
		/// <param name="embed">The embed to send. Best paired with <see cref="ExtensionMethods.GenerateDefaultEmbed">GenerateDefaultEmbed</see></param>
		/// <param name="mentions">A list of mentions to add to the user who issued the command.</param>
		/// <returns>The DiscordMessage sent</returns>
		public static async Task<DiscordMessage> SendMessage(CommandContext context, string content = null, DiscordEmbed embed = null, params IMention[] mentions)
		{
			if (content is null && embed is null) throw new ArgumentNullException(nameof(content), "Either content or embed needs to hold a value.");

			// Ping the person who invoked the command, and whoever else is required
			List<IMention> mentionList = new();
			mentionList.AddRange(mentions);
			mentionList.Add(new UserMention(context.User.Id));

			// Reply to the message that invoked this command
			DiscordMessageBuilder messageBuilder = new();
			_ = messageBuilder.WithReply(context.Message.Id, true);
			_ = messageBuilder.WithAllowedMentions(mentionList);
			_ = messageBuilder.HasTTS(false);
			if (content != null) messageBuilder.Content = content;
			if (embed != null) messageBuilder.Embed = embed;

			try
			{
				return await messageBuilder.SendAsync(context.Channel);
			}
			catch (UnauthorizedException)
			{
				return await context.Member.SendMessageAsync(messageBuilder);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
