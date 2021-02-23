using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class TempMute : BaseCommandModule
	{
		[Command("tempmute"), Description("Mutes a person temporarily."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("temp_mute", "tempsilence", "temp_silence"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, ExpandedTimeSpan muteTime, [RemainingText] string muteReason = Constants.MissingReason)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			DiscordRole muteRole = guild.MuteRole.GetRole(context.Guild);
			if (muteRole == null)
			{
				_ = await Program.SendMessage(context, Constants.MissingRole);
				return;
			}

			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been tempmuted by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)} for {Formatter.Bold(muteTime.ToString())}. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}");
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(muteRole, muteReason);
			}

			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user != null) user.IsMuted = true;

			Assignment assignment = new();
			assignment.AssignmentType = AssignmentType.TempMute;
			assignment.ChannelId = context.Channel.Id;
			assignment.Content = $"Tempmute issued for {victim.Id}";
			assignment.GuildId = context.Guild.Id;
			assignment.MessageId = context.Message.Id;
			assignment.SetOff = DateTime.Now + muteTime.TimeSpan;
			assignment.UserId = victim.Id;
			_ = Program.Database.Assignments.Add(assignment);

			_ = await Program.SendMessage(context, $"{victim.Mention} has been muted{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}", null, new UserMention(victim.Id));
		}
	}
}
