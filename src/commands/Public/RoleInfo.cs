using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;
using Tomoe.Utils;
using System;

namespace Tomoe.Commands.Public
{
	public class RoleInfo : BaseCommandModule
	{
		private static readonly Logger Logger = new("Commands.Public.RoleInfo");

		[Command("roleinfo"), Description("Gets information about a server role."), Aliases(new[] { "role_info", "ri" }), Priority(1)]
		public async Task ByName(CommandContext context, [Description("The role's name."), RemainingText] string roleName)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			roleName = roleName.Trim().ToLowerInvariant();
			List<DiscordRole> rolesInQuestion = new();
			// Check if it's the @everyone or @here roles.
			if (roleName == "everyone" || roleName == "@here")
			{
				Logger.Trace("Getting information on the everyone role!");
				await ByPing(context, context.Guild.GetRole(context.Guild.Id));
				return;
			}
			else
			{
				foreach (DiscordRole role in context.Guild.Roles.Values)
				{
					if (role.Name.ToLower() == roleName || role.Name.Contains(roleName))
					{
						Logger.Trace($"Found role {role.Id}...");
						rolesInQuestion.Add(role);
					}
				}
			}


			if (rolesInQuestion.Count == 0)
			{
				Logger.Trace("No role found!");
				_ = Program.SendMessage(context, $"There was no role called \"{roleName}\""); // No role was found. Inform the user.
			}
			else if (rolesInQuestion.Count == 1)
			{
				Logger.Trace($"Found only 1 role ({rolesInQuestion[0].Id})!");
				await ByPing(context, rolesInQuestion[0]);
			}
			else
			{
				Logger.Trace($"Found a total of {rolesInQuestion.Count} roles!");
				DiscordMessage message = Program.SendMessage(context, "Getting role permissions...");
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				Logger.Trace("Creating embed list...");
				List<Page> embeds = new();
				foreach (DiscordRole role in rolesInQuestion)
				{
					Logger.Trace("Creating embed...");
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
					Logger.Trace($"Getting members with role {role.Id}...");
					int roleMemberCount = 0;
					string roleUsers = string.Empty;
					foreach (DiscordMember member in context.Guild.Members.Values)
					{
						if (member.Roles.Contains(role) || role.Name == "@everyone")
						{
							roleMemberCount++;
							if (roleUsers.Length < 992)
							{
								roleUsers += $"{member.Mention} "; // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
							}
						}
					}
					_ = embed.AddField("**Members**", string.IsNullOrEmpty(roleUsers) ? "None" : roleUsers);
					Logger.Trace("Filling out description...");
					embed.Description = $"Id: **{role.Id}**\nName: **{role.Name}**\nCreation: **{role.CreationTimestamp}**\nPosition: **{role.Position}**\nColor: **{role.Color}**\nMentionable: **{role.IsMentionable}**\nHoisted: **{role.IsHoisted}**\nManaged: **{role.IsManaged}**\nPermissions: **{role.Permissions.ToPermissionString()}**\nMembers: **{roleMemberCount}**";
					Logger.Trace("Added embed to list...");
					embeds.Add(new Page(null, embed));
					Logger.Trace("Waiting 50ms to avoid breaking rate limits!");
					await Task.Delay(50);
				}
				Logger.Trace("Modifying message...");
				_ = await message.ModifyAsync($"{context.User.Mention}: Found a total of {embeds.Count} roles called {roleName.ToLowerInvariant()}.");
				Logger.Trace($"Sending paginated message with a total of {embeds.Count} pages!");
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds, default, PaginationBehaviour.Ignore);
				Logger.Trace("Embed sent!");
			}
		}

		[Command("roleinfo"), Priority(0)]
		public async Task ByPing(CommandContext context, [Description("The role id or pinged. Please refrain from pinging the roles.")] DiscordRole role)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Trace("Creating embed...");
			DiscordEmbedBuilder embed = new();
			embed.Author = new()
			{
				Name = context.User.Username,
				IconUrl = context.User.AvatarUrl,
				Url = context.User.AvatarUrl
			};
			embed.Color = role.Color;
			embed.Title = $"Role Info for **{role.Name}**";
			Logger.Trace($"Getting members with role {role.Id}...");
			int roleMemberCount = 0;
			string roleUsers = string.Empty;
			foreach (DiscordMember member in context.Guild.Members.Values)
			{
				if (member.Roles.Contains(role) || role.Name == "@everyone")
				{
					roleMemberCount++;
					if (roleUsers.Length < 992)
					{
						roleUsers += $"{member.Mention} "; // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
					}
				}
			}
			_ = embed.AddField("**Members**", string.IsNullOrEmpty(roleUsers) ? "None" : roleUsers);
			Logger.Trace("Filling out description...");
			embed.Description = $"Id: **{role.Id}**\nName: **{role.Name}**\nCreation: **{role.CreationTimestamp}**\nPosition: **{role.Position}**\nColor: **{role.Color}**\nMentionable: **{role.IsMentionable}**\nHoisted: **{role.IsHoisted}**\nManaged: **{role.IsManaged}**\nPermissions: **{role.Permissions.ToPermissionString()}**\nMembers: **{roleMemberCount}**";
			Logger.Trace("Sending embed...");
			_ = Program.SendMessage(context, embed.Build());
			Logger.Trace("Embed sent!");
		}
	}
}
