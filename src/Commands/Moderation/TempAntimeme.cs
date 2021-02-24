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
	public class TempmemeBan : BaseCommandModule
	{
		public Database Database { private get; set; }

		[Command("tempantimeme"), Description("Temporarily antimemes the victim."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("temp_antimeme", "tempanti_meme", "temp_anti_meme", "tempmemeban", "temp_memeban", "temp_meme_ban", "tempmeme_ban"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, ExpandedTimeSpan antimemeTime, [RemainingText] string antimemeReason = Constants.MissingReason)
		{
			DiscordRole antimemeRole = null;
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild != null) antimemeRole = guild.AntimemeRole.GetRole(context.Guild);
			if (antimemeRole == null)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been temporarily antimemed by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)} for {Formatter.Bold(antimemeTime.ToString())}. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: {Formatter.BlockCode(Formatter.Sanitize(antimemeReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(antimemeRole, antimemeReason);
			}
			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user != null) user.IsAntimemed = true;

			Assignment assignment = new();
			assignment.AssignmentType = AssignmentType.TempAntimeme;
			assignment.ChannelId = context.Channel.Id;
			assignment.Content = $"TempAntimeme issued for {victim.Id}";
			assignment.GuildId = context.Guild.Id;
			assignment.MessageId = context.Message.Id;
			assignment.SetOff = DateTime.Now + antimemeTime.TimeSpan;
			assignment.UserId = victim.Id;
			_ = Database.Assignments.Add(assignment);

			_ = await Program.SendMessage(context, $"{victim.Mention} has been antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Sanitize(antimemeReason))}", null, new UserMention(victim.Id));
		}
	}
}
