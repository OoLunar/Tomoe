using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using System.Threading.Tasks;

namespace Tomoe.Commands.Public
{
	public class TimeIs : BaseCommandModule
	{
		[Command("time_is")]
		[Description("Dev temp")]
		public async Task time_is(CommandContext context, ExpandedTimeSpan timeSpan)
		{
			Program.SendMessage(context, timeSpan.ToString());
		}
	}
}
