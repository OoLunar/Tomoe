using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Tomoe.Database;
using Tomoe.Utils;

using Microsoft.Extensions.Logging;

namespace Tomoe
{
	internal class Program
	{
		public const string MissingReason = "**[Notice: No reason was provided.]**";
		public const string MissingPermissions = "**[Denied: Missing permissions.]**";
		public const string NotAGuild = "**[Denied: Guild command.]**";
		public const string SelfAction = "**[Denied: Cannot execute on myself.]**";
		public const string Hierarchy = "**[Denied: Prevented by hierarchy.]**";
		public const string MissingRole = "**[Error: No role has been set!]**";
		public static DiscordShardedClient Client { get; private set; }
		public static readonly DatabaseLoader Database = new();
		private static readonly Logger _logger = new("Main");

		public static void Main() => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		public static async Task MainAsync()
		{
			await Config.Init();
			LoggerProvider loggerProvider = new();
			DiscordConfiguration discordConfiguration = new()
			{
				AutoReconnect = true,
				Token = Config.Token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = LogLevel.Information,
				UseRelativeRatelimit = true,
				MessageCacheSize = 512,
				LoggerFactory = loggerProvider,
			};

			Client = new(discordConfiguration);

			_ = Client.UseInteractivityAsync(new InteractivityConfiguration
			{
				// default pagination behaviour to just ignore the reactions
				PaginationBehaviour = PaginationBehaviour.WrapAround,
				// default timeout for other actions to 2 minutes
				Timeout = TimeSpan.FromMinutes(2),
				PaginationDeletion = PaginationDeletion.DeleteEmojis,
				PollBehaviour = PollBehaviour.DeleteEmojis
			});

			Client.MessageReactionAdded += (DiscordClient client, MessageReactionAddEventArgs eventArgs) => Task.Run(async () => Commands.Listeners.ReactionAdded.Handler(client, eventArgs));
			Client.GuildCreated += (DiscordClient client, GuildCreateEventArgs eventArgs) => Task.Run(async () => Commands.Listeners.GuildCreated.Handler(client, eventArgs));
			Client.GuildAvailable += (DiscordClient client, GuildCreateEventArgs eventArgs) => Task.Run(async () => Commands.Listeners.GuildAvailable.Handler(client, eventArgs));
			//Client.MessageCreated += (DiscordClient client, MessageCreateEventArgs eventArgs) => Task.Run(async () => Commands.Listeners.MessageRecieved.Handler(client, eventArgs));
			Client.Ready += (DiscordClient client, ReadyEventArgs eventArgs) => Task.Run(async () => Events.OnReady(client, eventArgs));
			await CommandService.Launch(Client);

			await Client.StartAsync();
			_logger.Info("Starting routines...");
			Commands.Public.Reminders.StartRoutine();
			_logger.Info("Started.");
			await Task.Delay(-1);
		}

		public static DiscordMessage SendMessage(CommandContext context, string content = null, DiscordEmbed embed = null, params IMention[] mentions)
		{
			List<IMention> mentionList = new List<IMention>();
			mentionList.AddRange(mentions);
			mentionList.Add(new UserMention(context.User.Id));
			DiscordMessageBuilder messageBuilder = new();
			messageBuilder.WithReply(context.Message.Id, true);
			messageBuilder.WithAllowedMentions(mentionList);
			messageBuilder.HasTTS(false);
			if (content == null && embed == null) throw new ArgumentNullException("Either content or embed needs to hold a value.");
			if (content != null) messageBuilder.Content = content;
			if (embed == null) messageBuilder.Embed = embed;
			try
			{
				return messageBuilder.SendAsync(context.Channel).GetAwaiter().GetResult();
			}
			catch (UnauthorizedException)
			{
				return context.Member.SendMessageAsync(messageBuilder).GetAwaiter().GetResult();
			}
		}
	}
}
