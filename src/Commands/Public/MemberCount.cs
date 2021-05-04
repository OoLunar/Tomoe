using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;

namespace Tomoe.Commands.Public
{
	public class MemberCount : BaseCommandModule
	{
		[Command("member_count"), Description("Sends the approximate member count."), Aliases("membercount", "mc")]
		public async Task Overload(CommandContext context) => await Program.SendMessage(context, $"Approximate member count: {context.Guild.MemberCount.ToMetric()}");
	}
}
