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
	public class Pardon : BaseCommandModule
	{
		[Command("pardon"), Description("Drops a strike."), Punishment]
		public async Task User(CommandContext context, int strikeId, [RemainingText] string pardonReason = Constants.MissingReason)
		{
			Strike droppedStrike = Program.Database.Strikes.Drop(strikeId, pardonReason).Value;

			bool sentDm = false;
			DiscordMember guildVictim = (await context.Client.GetUserAsync(droppedStrike.VictimId)).GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"Strike #{droppedStrike.StrikeCount} has been dropped by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}\nContext: {droppedStrike.JumpLink}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }

			_ = Program.SendMessage(context, $"Case #{droppedStrike.Id} has been dropped, <@{droppedStrike.VictimId}> has been pardoned{(sentDm ? '.' : " (Failed to DM).")}Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}", null, new UserMention(droppedStrike.VictimId));
		}
	}
}
