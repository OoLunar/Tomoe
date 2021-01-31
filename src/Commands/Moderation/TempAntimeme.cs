using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Database.Interfaces;

namespace Tomoe.Commands.Moderation
{
	public class TempmemeBan : BaseCommandModule
	{
		[Command("tempantimeme"), Description("Temporarily antimemes the victim."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("temp_antimeme", "tempanti_meme", "temp_anti_meme", "tempmemeban", "temp_memeban", "temp_meme_ban", "tempmeme_ban"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, ExpandedTimeSpan muteTime, [RemainingText] string antimemeReason = Constants.MissingReason)
		{
			DiscordRole antiMemeRole = Program.Database.Guild.AntimemeRole(context.Guild.Id).GetRole(context.Guild);
			if (antiMemeRole == null)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been temporarily antimemed by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)} for {Formatter.Bold(muteTime.ToString())}. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: {Formatter.BlockCode(Formatter.Sanitize(antimemeReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(antiMemeRole, antimemeReason);
			}
			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, true);
			Program.Database.Assignments.Create(AssignmentType.TempMute, context.Guild.Id, context.Channel.Id, context.Message.Id, victim.Id, DateTime.Now + muteTime.TimeSpan, DateTime.Now, $"{victim.Id} tempmuted in {context.Guild.Id}");
			_ = await Program.SendMessage(context, $"{victim.Mention} has been antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Sanitize(antimemeReason))}", null, new UserMention(victim.Id));
		}
	}
}
