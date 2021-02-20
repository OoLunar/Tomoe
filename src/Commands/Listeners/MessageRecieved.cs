using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

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
			if (eventArgs.Author == _client.CurrentUser) return;
			_ = await eventArgs.Channel.SendMessageAsync(Program.Database.Guild.AntiInvite(eventArgs.Guild.Id).ToString() + "Fired!");
			int maxMentions = Program.Database.Guild.MaxMentions(eventArgs.Guild.Id);
			int maxLines = Program.Database.Guild.MaxLines(eventArgs.Guild.Id);

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

			if (Program.Database.Guild.AntiInvite(eventArgs.Guild.Id))
			{
				Match messageInvites = _regex.Match(eventArgs.Message.Content);
				if (messageInvites.Success)
				{
					CaptureCollection invites = messageInvites.Captures;
					foreach (Capture capture in invites)
					{
						if (!Program.Database.Guild.IsAllowedInvite(eventArgs.Guild.Id, capture.Value)) await eventArgs.Message.DeleteAsync("Invite is not whitelisted.");
					}
				}
			}
		}
	}
}
