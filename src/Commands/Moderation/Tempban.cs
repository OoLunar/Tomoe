using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Tempban : BaseCommandModule
	{
		public Database Database { private get; set; }
		[Command("tempban"), Description("Temporarily bans someone from the server."), RequireGuild, RequireBotPermissions(Permissions.BanMembers), RequireUserPermissions(Permissions.BanMembers), Aliases("temp_ban"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, ExpandedTimeSpan banTime, int pruneDays = 7, [RemainingText] string banReason = Constants.MissingReason)
		{
			if (pruneDays < 7) pruneDays = 7;

			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been tempbanned by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)} for {Formatter.Bold(banTime.ToString())}. Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}");
				}
				catch (UnauthorizedException) { }
			await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason);

			Assignment assignment = new();
			assignment.AssignmentType = AssignmentType.TempBan;
			assignment.ChannelId = context.Channel.Id;
			assignment.Content = $"Tempban issued for {victim.Id}";
			assignment.GuildId = context.Guild.Id;
			assignment.MessageId = context.Message.Id;
			assignment.SetOff = DateTime.Now + banTime.TimeSpan;
			assignment.UserId = victim.Id;
			_ = Database.Assignments.Add(assignment);

			_ = await Program.SendMessage(context, $"{victim.Mention} has been temporarily banned{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}", null, new UserMention(victim.Id));
		}
	}
}
