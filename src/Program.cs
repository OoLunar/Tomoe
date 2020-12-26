using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Tomoe.Database;
using Tomoe.Utils;

namespace Tomoe
{
	internal class Program
	{
		public const string MissingReason = "**[No reason was provided.]**";
		public const string MissingPermissions = "**[Denied: Missing permissions.]**";
		public const string NotAGuild = "**[Denied: Guild command.]**";
		public const string SelfAction = "**[Denied: Cannot execute on myself.]**";
		public const string Hierarchy = "**[Denied: Prevented by hierarchy.]**";
		public const string MissingMuteRole = "**[Error: No mute role has been set!]**";
		public static DatabaseLoader Database = new DatabaseLoader();
		public static DiscordShardedClient Client;
		private static readonly Logger _logger = new Logger("Main");

		public static void Main() => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		public static async Task MainAsync()
		{
			Config.Init();
			LoggerProvider loggerProvider = new();
			DiscordConfiguration discordConfiguration = new DiscordConfiguration
			{
				AutoReconnect = true,
				Token = Config.Token,
				TokenType = TokenType.Bot,
				MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
				UseRelativeRatelimit = true,
				MessageCacheSize = 512,
				LoggerFactory = loggerProvider,
			};

			Client = new DiscordShardedClient(discordConfiguration);

			_ = Client.UseInteractivityAsync(new InteractivityConfiguration
			{
				// default pagination behaviour to just ignore the reactions
				PaginationBehaviour = PaginationBehaviour.WrapAround,
				// default timeout for other actions to 2 minutes
				Timeout = TimeSpan.FromMinutes(2)
			});

			Client.MessageReactionAdded += Commands.Listeners.ReactionAdded.Handler;
			Client.Ready += Events.OnReady;
			await CommandService.Launch(Client);

			await Client.StartAsync();
			_logger.Info("Starting routines...");
			Commands.Public.Reminders.StartRoutine();
			_logger.Info("Started.");
			await Task.Delay(-1);
		}

		public static DiscordMessage SendMessage(dynamic context, string content, ExtensionMethods.FilteringAction filteringAction = ExtensionMethods.FilteringAction.CodeBlocksEscape, List<IMention> mentionList = null)
		{
			if (mentionList == null)
			{
				mentionList = new List<IMention>();
			}

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

		public static DiscordMessage SendMessage(dynamic context, DiscordEmbed embed, List<IMention> mentionList = null)
		{
			if (mentionList == null)
			{
				mentionList = new List<IMention>();
			}

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

	public class ImageFormatConverter : IArgumentConverter<ImageFormat>
	{
		public Task<Optional<ImageFormat>> ConvertAsync(string value, CommandContext ctx)
		{
			switch (value.ToLowerInvariant())
			{
				case "png":
					return Task.FromResult(Optional.FromValue(ImageFormat.Png));
				case "jpeg":
					return Task.FromResult(Optional.FromValue(ImageFormat.Jpeg));
				case "webp":
					return Task.FromResult(Optional.FromValue(ImageFormat.WebP));
				case "gif":
					return Task.FromResult(Optional.FromValue(ImageFormat.Gif));
				case "unknown":
				case "auto":
					return Task.FromResult(Optional.FromValue(ImageFormat.Auto));
				default:
					return Task.FromResult(Optional.FromNoValue<ImageFormat>());
			}
		}
	}

	public static class ExtensionMethods
	{
		[Flags]
		public enum FilteringAction
		{
			CodeBlocksIgnore = 1,
			CodeBlocksEscape = 2,
			CodeBlocksZeroWidthSpace = 4
		}

		public static string Filter(this string modifyString, FilteringAction filteringAction = FilteringAction.CodeBlocksEscape)
		{
			if (string.IsNullOrEmpty(modifyString))
			{
				return null;
			}
			else if (filteringAction.HasFlag(FilteringAction.CodeBlocksZeroWidthSpace) || filteringAction.HasFlag(FilteringAction.CodeBlocksEscape))
			{
				return modifyString.Replace("`", "​`​"); // There are zero width spaces before and after the backtick.
			}
			else
			{
				return filteringAction.HasFlag(FilteringAction.CodeBlocksEscape) ? modifyString.Replace("\\", "\\\\").Replace("`", "\\`") : modifyString;
			}
		}

		public static string GetCommonName(this DiscordMember guildMember) => guildMember == null ? null : guildMember.Nickname ?? guildMember.Username;
	}
}
