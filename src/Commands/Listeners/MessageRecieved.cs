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

		/// <summary>
		/// Handles max lines, max mentions and anti-invite.
		/// </summary>
		/// <param name="client">Used to grab guild settings and the user id.</param>
		/// <param name="eventArgs">Used to grab the guild, remove the message if required and potentionally strike the user.</param>
		/// <returns></returns>
		public static async Task Handler(DiscordClient client, MessageCreateEventArgs eventArgs)
		{
			if (eventArgs.Author.Id == client.CurrentUser.Id
				|| eventArgs.Guild == null
				|| eventArgs.Message.WebhookMessage
				|| eventArgs.Author.IsBot
				|| (eventArgs.Author.IsSystem.HasValue && eventArgs.Author.IsSystem.Value)
			) return;
			DiscordMember authorMember = await eventArgs.Author.Id.GetMember(eventArgs.Guild);

			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guildConfig != null
				|| guildConfig.IgnoredChannels.Contains(eventArgs.Channel.Id)
				|| authorMember.HasPermission(Permissions.ManageMessages)
			  	|| authorMember.HasPermission(Permissions.Administrator)
			  	|| eventArgs.Guild.OwnerId == eventArgs.Author.Id
			  	|| guildConfig.AdminRoles.ConvertAll(role => role.ToString()).Intersect(authorMember.Roles.ToList().ConvertAll(role => role.ToString())).Any()
		  	) return;

			int maxMentions = guildConfig.MaxMentions;
			int maxLines = guildConfig.MaxLines;

			if (maxMentions > -1 && eventArgs.MentionedUsers.Count + eventArgs.MentionedRoles.Count > maxMentions)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max mentions count.");
				DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max mention count. Please refrain from spamming pings.");
				//TODO: Autostrikes
			}

			if (maxLines > -1 && eventArgs.Message.Content.Split('\n').Length > maxLines)
			{
				await eventArgs.Message.DeleteAsync("Exceeded max line count.");
				DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Message deleted due to it exceeding the max lines count. Please refrain from spamming chat.");
				//TODO: Autostrikes
				await Task.Delay(TimeSpan.FromSeconds(5));
				try { await message.DeleteAsync("Timed message"); }
				catch (NotFoundException) { }
			}

			if (guildConfig.AntiInvite)
			{
				Match messageInvites = InviteRegex.Match(eventArgs.Message.Content);
				if (messageInvites.Success)
				{
					CaptureCollection invites = messageInvites.Captures;
					foreach (Capture capture in invites)
					{
						if (!guildConfig.AllowedInvites.Contains(capture.Value))
						{
							await eventArgs.Message.DeleteAsync($"Invite {Formatter.InlineCode(capture.Value)} is not whitelisted.");
						}
					}
				}
			}
		}
	}
}
