using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using Humanizer;
using Tomoe.Attributes;

namespace Tomoe.Events
{
	public class CommandErrored
	{
		[SubscribeToEvent(nameof(CommandsNextExtension.CommandErrored))]
		public static Task CommandErroredAsync(CommandsNextExtension commandsNextExtension, CommandErrorEventArgs eventArgs)
		{
			StringBuilder stringBuilder = new();
			switch (eventArgs.Exception)
			{
				case DiscordException discordException:
					stringBuilder.AppendFormat("Bot 422: Discord API returned HTTP {0}, {1}", discordException.WebResponse.ResponseCode, discordException.JsonMessage);
					break;
				case ChecksFailedException checksFailedException:
					IEnumerable<IGrouping<Type, CheckBaseAttribute>> groupedChecks = checksFailedException.FailedChecks.GroupBy(x => x.GetType());
					foreach (IGrouping<Type, CheckBaseAttribute> group in groupedChecks)
					{
						switch (group.Key)
						{
							case Type when group.Key == typeof(RequireUserPermissionsAttribute):
								IEnumerable<RequireUserPermissionsAttribute> permissions = group.Select(x => x as RequireUserPermissionsAttribute)!;
								Permissions missingPermissions = permissions.Select(x => x.Permissions).Aggregate((x, y) => x | y);
								stringBuilder.AppendLine($"Bot 403, You're Missing Permissions: {missingPermissions.Humanize()}.");
								break;
							case Type when group.Key == typeof(RequireBotPermissionsAttribute):
								IEnumerable<RequireBotPermissionsAttribute> botPermissions = group.Select(x => x as RequireBotPermissionsAttribute)!;
								Permissions missingBotPermissions = botPermissions.Select(x => x.Permissions).Aggregate((x, y) => x | y);
								stringBuilder.AppendLine($"Bot 403, I'm Missing Permissions: {missingBotPermissions.Humanize()}.");
								break;
							case Type when group.Key == typeof(RequireGuildAttribute):
								stringBuilder.AppendLine("Bot 405, This Is A Guild Command.");
								break;
							case Type when group.Key == typeof(RequireDirectMessageAttribute):
								stringBuilder.AppendLine("Bot 405, This Is A DM Command.");
								break;
							default:
								stringBuilder.AppendLine($"Bot 500, {group.Key.Name}");
								break;
						}
					}
					break;
				case CommandNotFoundException commandNotFoundException:
					stringBuilder.AppendLine($"Bot 404, {Formatter.InlineCode(commandNotFoundException.CommandName)} was not found.");
					break;
				case Exception:
					stringBuilder.AppendLine($"Bot 500, {eventArgs.Exception.GetType().Name}: {eventArgs.Exception.Message}");
					break;
			}

			return eventArgs.Context.RespondAsync(stringBuilder.ToString());
		}
	}
}
