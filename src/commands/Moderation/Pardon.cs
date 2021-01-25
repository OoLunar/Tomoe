using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Tomoe.Database.Interfaces;

namespace Tomoe.Commands.Moderation
{
	public class Pardon : BaseCommandModule
	{
		[Command("pardon"), Description("Drops a strike.")]
		public async Task Person(CommandContext context, int strikeId, [RemainingText] string pardonReason = Program.MissingReason)
		{
			Strike droppedStrike = Program.Database.Strikes.Drop(strikeId, pardonReason).Value;

			DiscordMember guildVictim = await context.Guild.GetMemberAsync(droppedStrike.VictimId);
			bool sentDm = true;
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					await guildVictim.SendMessageAsync($"Strike #{droppedStrike.StrikeCount} has been dropped by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{pardonReason ?? Program.MissingReason}\n```\nContext: {droppedStrike.JumpLink}");
					sentDm = true;
				}
				catch (UnauthorizedException)
				{
					sentDm = false;
				}

			_ = Program.SendMessage(context, $"Case #{droppedStrike.Id} has been dropped, <@{droppedStrike.VictimId}> has been pardoned{(sentDm ? '.' : " (Failed to DM).")}Reason: ```\n{pardonReason ?? Program.MissingReason}\n```", null, new UserMention(droppedStrike.VictimId));
		}
	}
}
