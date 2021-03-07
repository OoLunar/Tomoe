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

namespace Tomoe.Commands.Public
{
	public class RoleInfo : BaseCommandModule
	{
		[Command("roleinfo"), Description("Gets information about a server role."), Aliases("role_info", "ri"), Priority(0)]
		public async Task Overload(CommandContext context, [Description("The role's name."), RemainingText] string roleName)
		{
			roleName = roleName.Trim().ToLowerInvariant();
			List<DiscordRole> rolesInQuestion = new();
			// Check if it's the @everyone or @here roles.
			if (roleName is "everyone" or "@here")
			{
				await Overload(context, context.Guild.GetRole(context.Guild.Id));
				return;
			}
			else
				foreach (DiscordRole role in context.Guild.Roles.Values)
					if (role.Name.ToLowerInvariant() == roleName || role.Name.Contains(roleName))
						rolesInQuestion.Add(role);

			if (rolesInQuestion.Count == 0) _ = await Program.SendMessage(context, Formatter.Bold($"[Error: There was no role called \"{roleName}\"]")); // No role was found. Inform the user.
			else if (rolesInQuestion.Count == 1) await Overload(context, rolesInQuestion[0]);
			else
			{
				DiscordMessage message = await Program.SendMessage(context, "Getting role permissions...");
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				List<Page> embeds = new();
				foreach (DiscordRole role in rolesInQuestion)
				{
					DiscordEmbedBuilder embed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Role Info for {Formatter.Bold(role.Name)}");
					embed.Color = role.Color;
					embed.Footer = new()
					{
						Text = $"Page {embeds.Count + 1}"
					};
					int roleMemberCount = 0;
					StringBuilder roleUsers = new();
					foreach (DiscordMember member in context.Guild.Members.Values)
					{
						if (member.Roles.Contains(role) || role.Name == "@everyone")
						{
							roleMemberCount++;
							if (roleUsers.Length < 992) _ = roleUsers.Append($"{member.Mention} "); // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
						}
					}
					_ = embed.AddField(Formatter.Bold("Members"), roleUsers.Length == 0 ? "None" : roleUsers.ToString());
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
					embeds.Add(new(null, embed));
					await Task.Delay(50);
				}
				_ = await message.ModifyAsync($"{context.User.Mention}: Found a total of {embeds.Count} roles called {roleName.ToLowerInvariant()}.");
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds, default, PaginationBehaviour.Ignore);
			}
		}

		[Command("roleinfo"), Priority(1)]
		public async Task Overload(CommandContext context, [Description("The role id or pinged. Please refrain from pinging the roles.")] DiscordRole role)
		{
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Role Info for {Formatter.Bold(role.Name)}");
			embed.Color = role.Color;
			int roleMemberCount = 0;
			StringBuilder roleUsers = new();
			foreach (DiscordMember member in context.Guild.Members.Values)
			{
				if (member.Roles.Contains(role) || role.Name == "@everyone")
				{
					roleMemberCount++;
					if (roleUsers.Length < 992) _ = roleUsers.Append($"{member.Mention} "); // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.					}
				}
			}
			_ = embed.AddField(Formatter.Bold("Members"), roleUsers.Length == 0 ? "None" : roleUsers.ToString());
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
			_ = await Program.SendMessage(context, null, embed.Build());
		}
	}
}
