using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Raw : BaseCommandModule
	{
		private static readonly Logger Logger = new("Commands.Public.Raw");
		[Command("raw"), Description("Gets the raw version of the message provided.")]
		public async Task Message(CommandContext context, [Description("The message id or jumplink to the message.")] DiscordMessage message)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			if (message.Content == null && message.Embeds.Count < 0)
			{
				Logger.Trace("Refusing to get contents of just an embed...");
				_ = Program.SendMessage(context, "**[Error: Cannot get the raw version of an embed!]**");
			}
			else
			{
				Logger.Trace($"Escaping characters...");
				string escapedContent = message.Content.Replace("\\", "\\\\").Replace("`", "\\`").Replace("_", "\\_").Replace("~", "\\~").Replace("<", "\\<").Replace(">", "\\>");
				Logger.Trace("Escaped characters!");
				_ = await context.RespondAsync($"{escapedContent}\n\n{context.User.Mention}{(message.Embeds.Count < 0 ? ": **[Notice: Cannot get the raw version of an embed!]**" : null)}");
				Logger.Trace("Message sent!");
			}
		}
	}
}
