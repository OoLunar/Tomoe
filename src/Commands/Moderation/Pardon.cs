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
		public async Task User(CommandContext context, Strike strike, [RemainingText] string pardonReason = Constants.MissingReason)
		{
			Strikes strikes = new();
			strikes.Database = Database;
			await strikes.BeforeExecutionAsync(context);
			await strikes.Drop(context, strike, pardonReason);
			await strikes.AfterExecutionAsync(context);
		}
	}
}
