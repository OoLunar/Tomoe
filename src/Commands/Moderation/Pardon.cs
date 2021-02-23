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
	public class Pardon : BaseCommandModule
	{
		[Command("pardon"), Description("Drops a strike."), Punishment]
		public async Task User(CommandContext context, int strikeId, [RemainingText] string pardonReason = Constants.MissingReason)
		{
			Strike droppedStrike = await Program.Database.Strikes.FirstOrDefaultAsync(strike => strike.Id == strikeId);
			droppedStrike.Dropped = true;
			droppedStrike.Reason.Add(pardonReason.Trim());
			droppedStrike.VictimMessaged = false;

			DiscordMember guildVictim = (await context.Client.GetUserAsync(droppedStrike.VictimId)).GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"Strike #{strikeId} has been dropped by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}\nContext: {droppedStrike.JumpLink}");
					droppedStrike.VictimMessaged = true;
				}
				catch (UnauthorizedException) { }

			_ = await Program.SendMessage(context, $"Case #{strikeId} has been dropped, <@{droppedStrike.VictimId}> has been pardoned{(droppedStrike.Dropped ? '.' : " (Failed to DM).")}Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}", null, new UserMention(droppedStrike.VictimId));
		}
	}
}
