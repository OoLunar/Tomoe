using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class RoleInfo : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.RoleInfo");

		[Command("roleinfo"), Description("Gets information about a server role."), Aliases("role_info", "ri"), Priority(0)]
		public async Task ByName(CommandContext context, [Description("The role's name."), RemainingText] string roleName)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			roleName = roleName.Trim().ToLowerInvariant();
			List<DiscordRole> rolesInQuestion = new();
			// Check if it's the @everyone or @here roles.
			if (roleName is "everyone" or "@here")
			{
				_logger.Trace("Getting information on the everyone role!");
				await ByPing(context, context.Guild.GetRole(context.Guild.Id));
				return;
			}
			else
			{
				foreach (DiscordRole role in context.Guild.Roles.Values)
				{
					if (role.Name.ToLowerInvariant() == roleName || role.Name.Contains(roleName))
					{
						_logger.Trace($"Found role {role.Id}...");
						rolesInQuestion.Add(role);
					}
				}
			}


			if (rolesInQuestion.Count == 0)
			{
				_logger.Trace("No role found!");
				_ = Program.SendMessage(context, Formatter.Bold($"[Error: There was no role called \"{roleName}\"]")); // No role was found. Inform the user.
			}
			else if (rolesInQuestion.Count == 1)
			{
				_logger.Trace($"Found only 1 role ({rolesInQuestion[0].Id})!");
				await ByPing(context, rolesInQuestion[0]);
			}
			else
			{
				_logger.Trace($"Found a total of {rolesInQuestion.Count} roles!");
				DiscordMessage message = Program.SendMessage(context, "Getting role permissions...");
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				_logger.Trace("Creating embed list...");
				List<Page> embeds = new();
				foreach (DiscordRole role in rolesInQuestion)
				{
					_logger.Trace("Creating embed...");
					DiscordEmbedBuilder embed = new();
					embed.Author = new()
					{
						Name = context.User.Username,
						IconUrl = context.User.AvatarUrl,
						Url = context.User.AvatarUrl
					};
					embed.Color = role.Color;
					embed.Title = $"Role Info for {Formatter.Bold(role.Name)}";
					embed.Footer = new()
					{
						Text = $"Page {embeds.Count + 1}"
					};
					_logger.Trace($"Getting members with role {role.Id}...");
					int roleMemberCount = 0;
					StringBuilder roleUsers = new();
					foreach (DiscordMember member in context.Guild.Members.Values)
					{
						if (member.Roles.Contains(role) || role.Name == "@everyone")
						{
							roleMemberCount++;
							if (roleUsers.Length < 992)
							{
								_ = roleUsers.Append($"{member.Mention} "); // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
							}
						}
					}
					_ = embed.AddField(Formatter.Bold("Members"), roleUsers.Length == 0 ? "None" : roleUsers.ToString());
					_logger.Trace("Filling out description...");
					StringBuilder roleInfo = new();
					_ = roleInfo.Append($"Id: {Formatter.Bold(role.Id.ToString(CultureInfo.InvariantCulture))}\n");
					_ = roleInfo.Append($"Name: {Formatter.Bold(role.Name.ToString())}\n");
					_ = roleInfo.Append($"Creation Timestamp: {Formatter.Bold(role.CreationTimestamp.ToString(CultureInfo.InvariantCulture))}\n");
					_ = roleInfo.Append($"Position: {Formatter.Bold(role.Position.ToString(CultureInfo.InvariantCulture))}\n");
					_ = roleInfo.Append($"Color: {Formatter.Bold(role.Color.ToString())}");
					_ = roleInfo.Append($"Mentionable: {Formatter.Bold(role.IsMentionable.ToString())}\n");
					_ = roleInfo.Append($"Hoisted: {Formatter.Bold(role.IsHoisted.ToString())}\n");
					_ = roleInfo.Append($"Managed: {Formatter.Bold(role.IsManaged.ToString())}\n");
					_ = roleInfo.Append($"Permissions: {Formatter.Bold(role.Permissions.ToPermissionString())}\n");
					_ = roleInfo.Append($"Member Count: {Formatter.Bold(roleMemberCount.ToString(CultureInfo.InvariantCulture))}");
					embed.Description = roleInfo.ToString();
					_logger.Trace("Added embed to list...");
					embeds.Add(new(null, embed));
					_logger.Trace("Waiting 50ms to avoid breaking rate limits!");
					await Task.Delay(50);
				}
				_logger.Trace("Modifying message...");
				_ = await message.ModifyAsync($"{context.User.Mention}: Found a total of {embeds.Count} roles called {roleName.ToLowerInvariant()}.");
				_logger.Trace($"Sending paginated message with a total of {embeds.Count} pages!");
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds, default, PaginationBehaviour.Ignore);
				_logger.Trace("Embed sent!");
			}
		}

		[Command("roleinfo"), Priority(1)]
		public async Task ByPing(CommandContext context, [Description("The role id or pinged. Please refrain from pinging the roles.")] DiscordRole role)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace("Creating embed...");
			DiscordEmbedBuilder embed = new();
			embed.Author = new()
			{
				Name = context.User.Username,
				IconUrl = context.User.AvatarUrl,
				Url = context.User.AvatarUrl
			};
			embed.Color = role.Color;
			embed.Title = $"Role Info for **{role.Name}**";
			_logger.Trace($"Getting members with role {role.Id}...");
			int roleMemberCount = 0;
			StringBuilder roleUsers = new();
			foreach (DiscordMember member in context.Guild.Members.Values)
			{
				if (member.Roles.Contains(role) || role.Name == "@everyone")
				{
					roleMemberCount++;
					if (roleUsers.Length < 992)
					{
						_ = roleUsers.Append($"{member.Mention} "); // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
					}
				}
			}
			_ = embed.AddField(Formatter.Bold("Members"), roleUsers.Length == 0 ? "None" : roleUsers.ToString());
			_logger.Trace("Filling out description...");
			StringBuilder roleInfo = new();
			_ = roleInfo.Append($"Id: {Formatter.Bold(role.Id.ToString(CultureInfo.InvariantCulture))}\n");
			_ = roleInfo.Append($"Name: {Formatter.Bold(role.Name.ToString())}\n");
			_ = roleInfo.Append($"Creation Timestamp: {Formatter.Bold(role.CreationTimestamp.ToString(CultureInfo.InvariantCulture))}\n");
			_ = roleInfo.Append($"Position: {Formatter.Bold(role.Position.ToString(CultureInfo.InvariantCulture))}\n");
			_ = roleInfo.Append($"Color: {Formatter.Bold(role.Color.ToString())}");
			_ = roleInfo.Append($"Mentionable: {Formatter.Bold(role.IsMentionable.ToString())}\n");
			_ = roleInfo.Append($"Hoisted: {Formatter.Bold(role.IsHoisted.ToString())}\n");
			_ = roleInfo.Append($"Managed: {Formatter.Bold(role.IsManaged.ToString())}\n");
			_ = roleInfo.Append($"Permissions: {Formatter.Bold(role.Permissions.ToPermissionString())}\n");
			_ = roleInfo.Append($"Member Count: {Formatter.Bold(roleMemberCount.ToString(CultureInfo.InvariantCulture))}");
			embed.Description = roleInfo.ToString();
			_logger.Trace("Sending embed...");
			_ = Program.SendMessage(context, null, embed.Build());
			_logger.Trace("Embed sent!");
		}
	}
}
