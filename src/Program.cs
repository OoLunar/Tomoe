using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Db;
using Tomoe.Utils;
using Serilog;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Tomoe
{
	public class Program
	{
		public static DiscordShardedClient Client { get; private set; }
		public static IServiceProvider ServiceProvider { get; private set; }

		public static void Main() => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		public static async Task MainAsync()
		{
			await Config.Init();
			string outputTemplate = Config.Logger.ShowId ? "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}" : "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
			LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
				.Enrich.WithThreadId()
				.MinimumLevel.Is(Config.Logger.Tomoe)
				.MinimumLevel.Override("DSharpPlus", Config.Logger.Discord)
				.MinimumLevel.Override("Microsoft", Config.Logger.Database)
				.WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: outputTemplate);
			if (Config.Logger.SaveToFile) _ = loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToLocalTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'", CultureInfo.InvariantCulture)}", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate);
			Log.Logger = loggerConfiguration.CreateLogger();
			Console.CancelKeyPress += Quit.ConsoleShutdown;
			ServiceCollection services = new();
			_ = services.AddDbContext<Database>();
			_ = services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger));
			ServiceProvider = services.BuildServiceProvider();
			_ = await ServiceProvider.GetService<Database>().Database.EnsureCreatedAsync();
			DiscordConfiguration discordConfiguration = new()
			{
				AutoReconnect = true,
				Token = Config.Token,
				TokenType = TokenType.Bot,
				UseRelativeRatelimit = false,
				MessageCacheSize = 512,
				LoggerFactory = ServiceProvider.GetService<ILoggerFactory>()
			};

			Client = new(discordConfiguration);
			Client.MessageCreated += CommandHandler.Handler;
			Client.MessageReactionAdded += Commands.Listeners.ReactionAdded.Handler;
			Client.GuildCreated += Commands.Listeners.GuildCreated.Handler;
			Client.GuildAvailable += Commands.Listeners.GuildAvailable.Handler;
			Client.GuildMemberAdded += Commands.Listeners.GuildMemberAdded.Handler;
			Client.GuildMemberUpdated += Commands.Listeners.GuildMemberUpdated.Handler;
			Client.GuildMemberRemoved += Commands.Listeners.GuildMemberRemoved.Handler;
			Client.ChannelCreated += Commands.Listeners.ChannelCreated.Handler;
			Client.MessageCreated += Commands.Listeners.MessageRecieved.Handler;
			Client.Ready += Commands.Listeners.OnReady.Handler;
			await CommandService.Launch(Client, ServiceProvider);
			await Client.StartAsync();
			Commands.Public.Assignments.StartRoutine();
			await Task.Delay(-1);
		}

		public static async Task<DiscordMessage> SendMessage(CommandContext context, string content = null, DiscordEmbed embed = null, params IMention[] mentions)
		{
			List<IMention> mentionList = new();
			mentionList.AddRange(mentions);
			mentionList.Add(new UserMention(context.User.Id));
			DiscordMessageBuilder messageBuilder = new();
			_ = messageBuilder.WithReply(context.Message.Id, true);
			_ = messageBuilder.WithAllowedMentions(mentionList);
			_ = messageBuilder.HasTTS(false);
			if (content == null && embed == null) throw new ArgumentNullException(nameof(content), "Either content or embed needs to hold a value.");
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
		}
	}
}
