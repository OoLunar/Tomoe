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
	public class Ban : BaseCommandModule
	{
		public ILogger<Ban> Logger { private get; init; } = null!;

		[Command("ban"), Description("Bans a user or member from the server."), RequireGuild, RequirePermissions(Permissions.BanMembers)]
		public async Task BanAsync(CommandContext context, [Description("Who's being banned.")] DiscordUser offender, [Description("Delete their messages within the past X days.")] int deleteDays = 1, [Description("Why they're being banned."), RemainingText] string reason = Constants.NoReasonSpecified)
		{
			// Check if the user is already banned
			try
			{
				await context.Guild.GetBanAsync(offender.Id);
				await context.RespondAsync($"[Error]: {offender.Mention} is already banned!");
				return;
			}
			catch (NotFoundException) { }

			bool successfullyMessaged = false;
			if (offender is DiscordMember memberOffender)
			{
				if (!context.Member!.CanExecute(Permissions.BanMembers, memberOffender))
				{
					await context.RespondAsync($"[Error]: You cannot ban {memberOffender.Mention} due to Discord permissions!");
					return;
				}
				else if (!context.Guild.CurrentMember.CanExecute(Permissions.BanMembers, memberOffender))
				{
					await context.RespondAsync($"[Error]: I cannot ban {memberOffender.Mention} due to Discord permissions!");
					return;
				}

				try
				{
					DiscordDmChannel dmChannel = await memberOffender.CreateDmChannelAsync();
					await dmChannel.SendMessageAsync($"You have been banned from {context.Guild.Name} ({context.Guild.Id}) by {context.Member!.Username}#{context.Member.Discriminator} ({context.Member.Id}) for the following reason:\n{reason}");
					successfullyMessaged = true;
				}
				catch (DiscordException) { }
			}

			try
			{
				await context.Guild.BanMemberAsync(offender.Id, deleteDays, reason);
			}
			catch (DiscordException error)
			{
				await context.RespondAsync($"[Error]: Failed to ban {offender.Mention}. Error: (HTTP {error.WebResponse.ResponseCode}) {error.JsonMessage}");
				Logger.LogWarning(error, "Failed to ban {Offender} from guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", offender.Id, context.Guild.Id, error.WebResponse.ResponseCode, error.JsonMessage);
				return;
			}

			await context.RespondAsync(Formatter.Bold($"{offender.Mention} ({offender.Id}) has been banned{(successfullyMessaged ? null : " (Failed to DM)")}!"));
		}
	}
}
