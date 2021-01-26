using System;
using System.Collections.Generic;
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

		[Command("roleinfo"), Description("Gets information about a server role."), Aliases(new[] { "role_info", "ri" }), Priority(0)]
		public async Task ByName(CommandContext context, [Description("The role's name."), RemainingText] string roleName)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			roleName = roleName.Trim().ToLowerInvariant();
			List<DiscordRole> rolesInQuestion = new();
			// Check if it's the @everyone or @here roles.
			if (roleName == "everyone" || roleName == "@here")
			{
				_logger.Trace("Getting information on the everyone role!");
				await ByPing(context, context.Guild.GetRole(context.Guild.Id));
				return;
			}
			else
			{
				foreach (DiscordRole role in context.Guild.Roles.Values)
				{
					if (role.Name.ToLower() == roleName || role.Name.Contains(roleName))
					{
						_logger.Trace($"Found role {role.Id}...");
						rolesInQuestion.Add(role);
					}
				}
			}


			if (rolesInQuestion.Count == 0)
			{
				_logger.Trace("No role found!");
				_ = Program.SendMessage(context, $"There was no role called \"{roleName}\""); // No role was found. Inform the user.
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
					embed.Title = $"Role Info for **{role.Name}**";
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
					_ = embed.AddField("**Members**", roleUsers.Length == 0 ? "None" : roleUsers.ToString());
					_logger.Trace("Filling out description...");
					embed.Description = $"Id: **{role.Id}**\nName: **{role.Name}**\nCreation: **{role.CreationTimestamp}**\nPosition: **{role.Position}**\nColor: **{role.Color}**\nMentionable: **{role.IsMentionable}**\nHoisted: **{role.IsHoisted}**\nManaged: **{role.IsManaged}**\nPermissions: **{role.Permissions.ToPermissionString()}**\nMembers: **{roleMemberCount}**";
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
			_ = embed.AddField("**Members**", roleUsers.Length == 0 ? "None" : roleUsers.ToString());
			_logger.Trace("Filling out description...");
			embed.Description = $"Id: **{role.Id}**\nName: **{role.Name}**\nCreation: **{role.CreationTimestamp}**\nPosition: **{role.Position}**\nColor: **{role.Color}**\nMentionable: **{role.IsMentionable}**\nHoisted: **{role.IsHoisted}**\nManaged: **{role.IsManaged}**\nPermissions: **{role.Permissions.ToPermissionString()}**\nMembers: **{roleMemberCount}**";
			_logger.Trace("Sending embed...");
			_ = Program.SendMessage(context, null, embed.Build());
			_logger.Trace("Embed sent!");
		}
	}
}
