using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Database.Interfaces;

namespace Tomoe.Commands.Moderation
{
	public class Pardon : BaseCommandModule
	{
		[Command("pardon"), Description("Drops a strike.")]
		public async Task Person(CommandContext context, int strikeId, [RemainingText] string pardonReason = Program.MissingReason)
		{
			Strike droppedStrike = Program.Database.Strikes.Drop(strikeId, pardonReason).Value;
			bool sentDm = true;

			try
			{
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(droppedStrike.VictimId);
				if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"Strike #{droppedStrike.StrikeCount} has been dropped by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{pardonReason.Filter() ?? Program.MissingReason}\n```\nContext: {droppedStrike.JumpLink}");

			}
			catch (NotFoundException)
			{
				sentDm = false;
			}
			catch (UnauthorizedException)
			{
				sentDm = false;
			}
			_ = Program.SendMessage(context, $"Case #{droppedStrike.Id} has been dropped, <@{droppedStrike.VictimId}> has been pardoned{(sentDm ? '.' : " (Failed to DM).")}Reason: ```\n{pardonReason.Filter(ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace) ?? Program.MissingReason}\n```", default, new List<IMention>() { new UserMention(droppedStrike.VictimId) });
		}
	}
}
