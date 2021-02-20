using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;
using Tomoe.Db;
using Tomoe.Utils;

namespace Tomoe.Commands.Listeners
{
	public class MessageRecieved
	{
		private static readonly Logger _logger = new("Commands.Listeners.GuildAvailable");
		private static readonly Regex _regex = new(@"(discord((app\.com|.com)\/invite|\.gg)\/[A-z]+)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

		public static async Task Handler(DiscordClient _client, MessageCreateEventArgs eventArgs)
		{
			_logger.Trace($"Recieved message in {eventArgs.Channel.Id} on {eventArgs.Guild.Id}");
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == eventArgs.Guild.Id);
			if (eventArgs.Author == _client.CurrentUser) return;
			int maxMentions = guild.MaxMentions;
			int maxLines = guild.MaxLines;

			if (maxMentions > -1 && eventArgs.MentionedUsers.Count + eventArgs.MentionedRoles.Count > maxMentions)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max mentions count.");
				_ = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max mention count. Please refrain from spamming pings.");
			}

			if (maxLines > -1 && eventArgs.Message.Content.Split('\n').Length > maxLines)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max line length.");
				_ = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max lines count. Please refrain from spamming pings.");
			}

			if (guild.AntiInvite)
			{
				Match messageInvites = _regex.Match(eventArgs.Message.Content);
				if (messageInvites.Success)
				{
					CaptureCollection invites = messageInvites.Captures;
					foreach (Capture capture in invites)
						if (!guild.AllowedInvites.Contains(capture.Value))
							await eventArgs.Message.DeleteAsync("Invite is not whitelisted.");
				}
			}
		}
	}
}
