using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Pardon : BaseCommandModule
	{
		public Database Database { private get; set; }

		[Command("pardon"), Description("Drops a strike."), Punishment]
		public async Task User(CommandContext context, int strikeId, [RemainingText] string pardonReason = Constants.MissingReason)
		{
			Strike strike = await Database.Strikes.FirstOrDefaultAsync(strike => strike.Id == strikeId);
			strike.Dropped = true;
			strike.Reason.Add(pardonReason.Trim());
			strike.VictimMessaged = false;

			DiscordMember guildVictim = (await context.Client.GetUserAsync(strike.VictimId)).GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"Strike #{strike.Id} has been dropped by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}\nContext: {strike.JumpLink}");
					strike.VictimMessaged = true;
				}
				catch (UnauthorizedException) { }

			_ = await Program.SendMessage(context, $"Case #{strike.Id} has been dropped, <@{strike.VictimId}> has been pardoned{(strike.Dropped ? '.' : " (Failed to DM).")}Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}", null, new UserMention(strike.VictimId));
		}
	}
}
