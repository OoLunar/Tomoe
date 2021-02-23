using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public
{
	public class Raw : BaseCommandModule
	{
		[Command("raw"), Description("Gets the raw version of the message provided."), Aliases("source")]
		public async Task Overload(CommandContext context, [Description("The message id or jumplink to the message.")] DiscordMessage message)
		{
			if (message.Content == string.Empty && message.Embeds.Count != 0) _ = await Program.SendMessage(context, Constants.RawEmbed);
			else _ = await Program.SendMessage(context, $"{Formatter.Sanitize(message.Content)}{(message.Embeds.Count != 0 ? '\n' + Constants.RawEmbed : null)}");
		}
	}
}
