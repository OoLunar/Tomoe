using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.EventArgs;

using Tomoe.Db;
using System.Linq;

namespace Tomoe.Commands.Listeners
{
	public class MessageRecieved
	{
		private static readonly Regex _regex = new(@"(discord((app\.com|.com)\/invite|\.gg)\/[A-z]+)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

		public static async Task Handler(DiscordClient _client, MessageCreateEventArgs eventArgs)
		{
			if (eventArgs.Author == _client.CurrentUser && eventArgs.Guild == null) return;
			Database Database = (Database)Program.ServiceProvider.GetService(typeof(Database));
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null || guild.IgnoredChannels.Contains(eventArgs.Channel.Id) || guild.AdminRoles.Cast<string>().Intersect(eventArgs.Author.GetMember(eventArgs.Guild).Roles.Cast<string>()).Any()) return;
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
				_ = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max lines count. Please refrain from spamming chat.");
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
