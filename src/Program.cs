using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.EventArgs;
using Tomoe.Database;
using Tomoe.Utils;
using DSharpPlus.CommandsNext;

namespace Tomoe
{
	internal class Program
	{
		public const string MissingReason = "**[No reason was provided.]**";
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
				MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
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
				Timeout = TimeSpan.FromMinutes(2)
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

		public static DiscordMessage SendMessage(CommandContext context, string content, ExtensionMethods.FilteringAction filteringAction = ExtensionMethods.FilteringAction.CodeBlocksEscape, List<IMention> mentionList = null)
		{
			if (mentionList == null) mentionList = new List<IMention>();
			mentionList.Add(new UserMention(context.User.Id));
			try
			{
				return context.RespondAsync($"{context.User.Mention}: {content.Filter(filteringAction)}", false, null, mentionList as IEnumerable<IMention>).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (DSharpPlus.Exceptions.UnauthorizedException)
			{
				return context.Member.SendMessageAsync($"Responding to <{context.Message.JumpLink}>: {content.Filter(filteringAction)}").ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}

		public static DiscordMessage SendMessage(CommandContext context, DiscordEmbed embed, List<IMention> mentionList = null)
		{
			if (mentionList == null) mentionList = new List<IMention>();
			mentionList.Add(new UserMention(context.User.Id));
			try
			{
				return context.RespondAsync($"{context.User.Mention}: ", false, embed, mentionList as IEnumerable<IMention>).ConfigureAwait(false).GetAwaiter().GetResult();
			}
			catch (DSharpPlus.Exceptions.UnauthorizedException)
			{
				return context.Member.SendMessageAsync($"Responding to <{context.Message.JumpLink}>: ", false, embed).ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
	}
}
