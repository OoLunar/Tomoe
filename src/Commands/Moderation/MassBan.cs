using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Tomoe.Models;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
	public class MassBan : BaseCommandModule
	{
		public DatabaseContext Database { private get; set; } = null!;

		[Command("massban"), Description("Bans all the specified users without DM'ing them and skipping the pre-banned check."), RequireGuild, RequirePermissions(Permissions.BanMembers)]
		public async Task MassBanAsync(CommandContext context, [RemainingText, Description("Whom to ban.")] params ulong[] userIds)
		{
			await context.TriggerTypingAsync();
			long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			int successfulBans = 0;
			ConcurrentDictionary<string, List<ulong>> errorsToUsersDict = new();

			Dictionary<ulong, DiscordMember> members = context.Guild.Members.Values.Where(x => userIds.Contains(x.Id)).ToDictionary(x => x.Id);
			Dictionary<ulong, MemberModel> dbMembers = Database.GuildMembers.Where(x => x.GuildId == context.Guild.Id && userIds.Except(members.Keys).Contains(x.UserId) && x.IsInGuild).ToDictionary(x => x.UserId);

			foreach (ulong userId in userIds)
			{
				if (members.TryGetValue(userId, out DiscordMember? member) && member != null)
				{
					if (!context.Member!.CanExecute(Permissions.BanMembers, member))
					{
						errorsToUsersDict.AddOrUpdate("Bot 403, You Cannot Ban Someone Of Higher Hierarchy", new List<ulong> { userId }, (key, list) =>
						{
							list.Add(userId);
							return list;
						});
					}
					else if (!context.Guild.CurrentMember.CanExecute(Permissions.BanMembers, member))
					{
						errorsToUsersDict.AddOrUpdate("Bot 403, I Cannot Ban Someone Of Higher Hierarchy", new List<ulong> { userId }, (key, list) =>
						{
							list.Add(userId);
							return list;
						});
					}
					continue;
				}
				else if (dbMembers.TryGetValue(userId, out MemberModel? dbMember) && dbMember != null)
				{
					IEnumerable<DiscordRole> sacrificesRoles = dbMember.Roles.Select(x => context.Guild.GetRole(x));
					if (!context.Member!.Roles.CanExecute(Permissions.BanMembers, sacrificesRoles))
					{
						errorsToUsersDict.AddOrUpdate("Bot 403, You Cannot Ban Someone Of Higher Hierarchy", new List<ulong> { userId }, (key, list) =>
						{
							list.Add(userId);
							return list;
						});
					}
					if (!context.Guild.CurrentMember.Roles.CanExecute(Permissions.BanMembers, sacrificesRoles))
					{
						errorsToUsersDict.AddOrUpdate("Bot 403, I Cannot Ban Someone Of Higher Hierarchy", new List<ulong> { userId }, (key, list) =>
						{
							list.Add(userId);
							return list;
						});
					}
					continue;
				}

				try
				{
					await context.Guild.BanMemberAsync(userId, 1, $"MassBan of {unixTimestamp}");
					successfulBans++;
				}
				catch (DiscordException error)
				{
					string index = $"HTTP {error.WebResponse.ResponseCode}: {error.JsonMessage}";
					if (errorsToUsersDict.TryGetValue(index, out List<ulong>? userIdsForError))
					{
						if (userIdsForError == null)
						{
							userIdsForError = new List<ulong>();
							errorsToUsersDict[index] = userIdsForError;
						}
						userIdsForError.Add(userId);
					}
					else
					{
						errorsToUsersDict[index] = new List<ulong> { userId };
					}
				}
			}

			StringBuilder stringBuilder = new($"{successfulBans.ToMetric(decimals: 2)} user{(successfulBans != 1 ? 's' : null)} ha{(successfulBans != 1 ? "ve" : "s")} been banned!");
			if (!errorsToUsersDict.IsEmpty)
			{
				stringBuilder.AppendLine(" Heads up, the following users were unable to be banned:");
				foreach ((string reason, List<ulong> failedUsers) in errorsToUsersDict)
				{
					stringBuilder.AppendLine($"- {reason}: {failedUsers.Aggregate(string.Empty, (current, userId) => current + $"`{userId}`, ")[..^2]}"); // [..^2] to remove the trailing comma and space
				}
			}

			await context.RespondAsync(stringBuilder.ToString());
		}
	}
}
