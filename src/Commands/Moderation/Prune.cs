using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class Prune : BaseCommandModule
	{
		[Command("prune")]
		[Description("Prunes members from the guild.")]
		[RequirePermissions(Permissions.KickMembers)]
		public async Task PruneAsync(CommandContext context)
		{
			await (await context.Guild.GetMemberAsync(278259852701335553)).RemoveAsync("Server prune, invoked by " + context.User.Mention);
			await context.Guild.PruneAsync(reason: "Server prune, invoked by " + context.User.Mention);
			await (await context.Guild.GetMemberAsync(445040384922615819)).RemoveAsync("Server prune, invoked by " + context.User.Mention);
			await context.RespondAsync("Server prune done!");
		}
	}
}
