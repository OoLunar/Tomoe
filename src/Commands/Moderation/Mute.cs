using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
	public class Mute : BaseCommandModule
	{
		public ILogger<Mute> Logger { private get; init; } = null!;

		[Command("mute"), Description("Mutes a user or member from the server."), RequireGuild, RequirePermissions(Permissions.ModerateMembers)]
		public async Task MuteUserAsync(CommandContext context, [Description("Who's getting muted?")] DiscordMember offender, [Description("How long will they be muted?")] TimeSpan timeSpan, [Description("Why are they getting muted?"), RemainingText] string reason = Constants.NoReasonSpecified)
		{
			// Check if the executing user can mute members.
			if (!context.Member!.CanExecute(Permissions.ModerateMembers, offender))
			{
				await context.RespondAsync($"[Error]: You cannot mute {offender.Mention} due to Discord permissions!");
				return;
			}
			// Check if the bot can mute members.
			else if (!context.Guild.CurrentMember.CanExecute(Permissions.ModerateMembers, offender))
			{
				await context.RespondAsync($"[Error]: I cannot mute {offender.Mention} due to Discord permissions!");
				return;
			}

			// Attempt to Dm the user.
			bool dmSuccess = true;
			try
			{
				DiscordDmChannel dmChannel = await offender.CreateDmChannelAsync();
				await dmChannel.SendMessageAsync($"You have been muted from {context.Guild.Name} ({context.Guild.Id}) by {context.Member!.Username}#{context.Member.Discriminator} ({context.Member.Id}) for the following reason:\n{reason}");
			}
			catch (DiscordException)
			{
				dmSuccess = false;
			}

			// Attempt to mute the user.
			try
			{
				await offender.TimeoutAsync(DateTime.UtcNow.Add(timeSpan), reason);
			}
			catch (DiscordException error)
			{
				await context.RespondAsync($"[Error]: Failed to mute {offender.Mention}. Error: (HTTP {error.WebResponse.ResponseCode}) {error.JsonMessage}");
				Logger.LogWarning(error, "Failed to mute {Offender} from guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", offender.Id, context.Guild.Id, error.WebResponse.ResponseCode, error.JsonMessage);
				return;
			}

			await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been muted{(dmSuccess ? null : " (Failed to DM)")}! The mute will be removed {Formatter.Timestamp(timeSpan)}!"));
		}
	}
}
