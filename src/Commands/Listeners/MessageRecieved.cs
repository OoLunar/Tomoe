using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class MessageRecieved
	{
		public static readonly Regex InviteRegex = new(@"disc(?:ord)?(?:(?:app)?\.com\/invite|(?:\.gg))\/([A-z0-9-]{2,})", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

		public static async Task Handler(DiscordClient client, MessageCreateEventArgs eventArgs)
		{
			if (eventArgs.Author.Id == client.CurrentUser.Id
				|| eventArgs.Guild == null
				|| eventArgs.Message.WebhookMessage
				|| eventArgs.Author.IsBot
				|| (eventArgs.Author.IsSystem.HasValue && eventArgs.Author.IsSystem.Value)
			) return;
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			DiscordMember authorMember = await eventArgs.Author.Id.GetMember(eventArgs.Guild);
			if (guild == null
				|| guild.IgnoredChannels.Contains(eventArgs.Channel.Id)
				|| authorMember.HasPermission(Permissions.ManageMessages)
			  	|| authorMember.HasPermission(Permissions.Administrator)
			  	|| eventArgs.Guild.OwnerId == eventArgs.Author.Id
			  	|| guild.AdminRoles.ConvertAll(role => role.ToString()).Intersect(authorMember.Roles.ToList().ConvertAll(role => role.ToString())).Any()
		  	) return;

			int maxMentions = guild.MaxMentions;
			int maxLines = guild.MaxLines;

			if (maxMentions > -1 && eventArgs.MentionedUsers.Count + eventArgs.MentionedRoles.Count > maxMentions)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max mentions count.");
				DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max mention count. Please refrain from spamming pings.");
				if (guild.StrikeAutomod) await Moderation.Strikes.ByProgram(eventArgs.Guild, eventArgs.Author, message.JumpLink, "Exceeded max mentions count. Please refrain from mass pinging.");
			}

			if (maxLines > -1 && eventArgs.Message.Content.Split('\n').Length > maxLines)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max line count.");
				DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max lines count. Please refrain from spamming chat.");
				if (guild.StrikeAutomod) await Moderation.Strikes.ByProgram(eventArgs.Guild, eventArgs.Author, message.JumpLink, "Exceeded max line count. Please refrain from spamming new lines.");
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
					{
						if (!guild.AllowedInvites.Contains(capture.Value))
						{
							await eventArgs.Message.DeleteAsync($"Invite {Formatter.InlineCode(capture.Value)} is not whitelisted.");
						}
					}
				}
			}
		}
	}
}
