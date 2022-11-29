using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Common
{
	public class MemberCount : BaseCommandModule
	{
		[Command("member_count"), Description("Sends the approximate member count."), Aliases("mc")]
		public Task MemberCountAsync(CommandContext context) => context.RespondAsync($"Member count: {Program.MemberCounts[context.Guild.Id].ToString("N0")}");
	}
}
