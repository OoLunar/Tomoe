using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public
{
	public class Raw : BaseCommandModule
	{
		[Command("raw"), Description("Gets the raw version of the message provided.")]
		public async Task Message(CommandContext context, [Description("The message id or jumplink to the message.")] DiscordMessage message) => Program.SendMessage(context, $"\n{message.Content}");
	}
}
