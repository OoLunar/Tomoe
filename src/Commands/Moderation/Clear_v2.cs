using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Tomoe.Commands.Moderation
{
	public class ClearV2 : BaseCommandModule
	{
		public ILogger<Clear> Logger { private get; init; } = null!;

		// We do this for the invite command. The clear command utilizes both role permissions and channel overwrites. If an admin really wants to lock down on permissions, I'd like for Tomoe not to struggle one bit.
		[RequirePermissions(Permissions.ManageMessages)]
		public void PseudoMethod() { }

		[Command("clear_v2"), Description("Clears messages from chat."), RequireGuild]
		public async Task ClearChannelAsync(CommandContext context, DiscordMessage firstMessage, DiscordMessage? lastMessage = null, [RemainingText] string? reason = null)
		{
			IEnumerable<DiscordMessage> messages = (await firstMessage.Channel.GetMessagesAfterAsync(firstMessage.Id)).Prepend(firstMessage);
			if (lastMessage != null)
			{
				messages = messages.OrderBy(x => x.CreationTimestamp).TakeWhile(m => m.Id != lastMessage.Id).Append(lastMessage);
			}

			await firstMessage.Channel.DeleteMessagesAsync(messages, reason ?? "No reason provided.");
			await context.RespondAsync($"{messages.Count():N0} deleted.");
		}
	}
}
