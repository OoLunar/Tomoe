using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;
using Tomoe.Utils;

namespace Tomoe
{
	public class Program
	{
		public static DiscordShardedClient Client { get; private set; }
		public static IServiceProvider ServiceProvider { get; private set; }
		private static readonly Logger _logger = new("Main");

		public static void Main() => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		public static async Task MainAsync()
		{
			await Config.Init();
			LoggerProvider loggerProvider = new();
			Console.CancelKeyPress += Quit.ConsoleShutdown;
			ServiceCollection services = new();
			_ = services.AddDbContext<Database>();
			ServiceProvider = services.BuildServiceProvider();
			_ = await ServiceProvider.GetService<Database>().Database.EnsureCreatedAsync();
			DiscordConfiguration discordConfiguration = new()
			{
				AutoReconnect = true,
				Token = Config.Token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = Config.LoggerConfig.Discord,
				UseRelativeRatelimit = false,
				MessageCacheSize = 512,
				LoggerFactory = loggerProvider
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
			_logger.Info("Starting routines...");
			Commands.Public.Assignments.StartRoutine();
			_logger.Info("Started.");
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
