using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.EventArgs;

using Tomoe.Db;
using System.Linq;
using System;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Listeners
{
	public class MessageRecieved
	{
		public static readonly Regex InviteRegex = new(@"disc(?:ord)?(?:(?:app)?\.com\/invite|(?:\.gg))\/([A-z0-9-]{2,})", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

		public static async Task Handler(DiscordClient _client, MessageCreateEventArgs eventArgs)
		{
			if (eventArgs.Author.Id == _client.CurrentUser.Id || eventArgs.Guild == null) return;
			Database Database = (Database)Program.ServiceProvider.GetService(typeof(Database));
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null
				|| guild.IgnoredChannels.Contains(eventArgs.Channel.Id)
				|| eventArgs.Author.GetMember(eventArgs.Guild).HasPermission(Permissions.ManageMessages)
				|| eventArgs.Author.GetMember(eventArgs.Guild).HasPermission(Permissions.Administrator)
				|| eventArgs.Guild.OwnerId == eventArgs.Author.Id
				|| guild.AdminRoles.Cast<string>().Intersect(eventArgs.Author.GetMember(eventArgs.Guild).Roles.Cast<string>()).Any()
			) return;
			int maxMentions = guild.MaxMentions;
			int maxLines = guild.MaxLines;

			if (maxMentions > -1 && eventArgs.MentionedUsers.Count + eventArgs.MentionedRoles.Count > maxMentions)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max mentions count.");
				DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max mention count. Please refrain from spamming pings.");
				if (guild.StrikeAutomod) await Moderation.Strikes.Automated(eventArgs.Guild, Database.Strikes, eventArgs.Author, message.JumpLink, "Exceeded max mentions count. Please refrain from mass pinging.");
			}

			if (maxLines > -1 && eventArgs.Message.Content.Split('\n').Length > maxLines)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max line count.");
				DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max lines count. Please refrain from spamming chat.");
				if (guild.StrikeAutomod) await Moderation.Strikes.Automated(eventArgs.Guild, Database.Strikes, eventArgs.Author, message.JumpLink, "Exceeded max line count. Please refrain from spamming new lines.");
				await Task.Delay(TimeSpan.FromSeconds(5));
				try { await message.DeleteAsync("Timed message"); }
				catch (NotFoundException) { }
			}

			if (guild.AntiInvite)
			{
				Match messageInvites = InviteRegex.Match(eventArgs.Message.Content);
				if (messageInvites.Success)
				{
					CaptureCollection invites = messageInvites.Captures;
					foreach (Capture capture in invites)
						if (!guild.AllowedInvites.Contains(capture.Value))
							await eventArgs.Message.DeleteAsync($"Invite {Formatter.InlineCode(capture.Value)} is not whitelisted.");
				}
			}
		}
	}
}
