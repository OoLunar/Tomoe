using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		private const string _COMMAND_NAME = "role_info";
		private const string _COMMAND_DESC = "Gets information about a server role.";
		private const string _ARG_ROLENAME_DESC = "The role's name.";
		private const string _ARG_ROLE_DESC = "The role id or pinged. Please refrain from pinging the roles.";
		private static Logger Logger = new Logger("Commands.Public.RoleInfo");

		[Command(_COMMAND_NAME), Description(_COMMAND_DESC), Aliases(new string[] { "roleinfo", "ri" }), Priority(1)]
		public async Task ByName(CommandContext context, [Description(_ARG_ROLENAME_DESC), RemainingText] string roleName)
		{
			roleName = roleName.ToLower();
			List<DiscordRole> rolesInQuestion = new List<DiscordRole>();
			// Check if it's the @everyone or @here roles.
			if (roleName == "everyone" || roleName == "@here")
			{
				await ByPing(context, context.Guild.GetRole(context.Guild.Id));
				return;
			}
			else
			{
				foreach (DiscordRole role in context.Guild.Roles.Values)
				{
					if (role.Name.ToLower() == roleName)
					{
						rolesInQuestion.Add(role);
					}
				}
			}


			if (rolesInQuestion.Count == 0)
			{
				_ = Program.SendMessage(context, $"There was no role called \"{roleName}\""); // No role was found. Inform the user.
			}
			else if (rolesInQuestion.Count == 1)
			{
				await ByPing(context, rolesInQuestion[0]);
			}
			else
			{
				DiscordMessage message = Program.SendMessage(context, "Getting role permissions...");
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				List<Page> embeds = new List<Page>();
				foreach (DiscordRole role in rolesInQuestion)
				{
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
					embed.Description = $"Id: **{role.Id}**\nName: **{role.Name}**\nCreation: **{role.CreationTimestamp}**\nPosition: **{role.Position}**\nColor: **{role.Color}**\nMentionable: **{role.IsMentionable}**\nHoisted: **{role.IsHoisted}**\nManaged: **{role.IsManaged}**\nPermissions: **{role.Permissions.ToPermissionString()}**\nMembers: **{roleMemberCount}**";
					embeds.Add(new Page(null, embed));
					await Task.Delay(50);
				}
				_ = await message.ModifyAsync($"{context.User.Mention}: Found a total of {embeds.Count} roles called {roleName.ToLowerInvariant()}.");
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds.Take(embeds.Count), default, PaginationBehaviour.Ignore);
			}
		}

		[Command(_COMMAND_NAME), Priority(0)]
		public async Task ByPing(CommandContext context, [Description(_ARG_ROLE_DESC)] DiscordRole role)
		{
			DiscordEmbedBuilder embed = new();
			embed.Author = new()
			{
				Name = context.User.Username,
				IconUrl = context.User.AvatarUrl,
				Url = context.User.AvatarUrl
			};
			embed.Color = role.Color;
			embed.Title = $"Role Info for **{role.Name}**";
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
			embed.Description = $"Id: **{role.Id}**\nName: **{role.Name}**\nCreation: **{role.CreationTimestamp}**\nPosition: **{role.Position}**\nColor: **{role.Color}**\nMentionable: **{role.IsMentionable}**\nHoisted: **{role.IsHoisted}**\nManaged: **{role.IsManaged}**\nPermissions: **{role.Permissions.ToPermissionString()}**\nMembers: **{roleMemberCount}**";
			_ = Program.SendMessage(context, embed.Build());
		}
	}
}
