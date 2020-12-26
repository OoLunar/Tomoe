using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public
{
	public class RoleInfo : BaseCommandModule
	{
		private const string _COMMAND_NAME = "role_info";
		private const string _COMMAND_DESC = "Gets information about a server role.";
		private const string _ARG_ROLENAME_DESC = "The role's name.";
		private const string _ARG_ROLE_DESC = "The role id or pinged. Please refrain from pinging the roles.";

		[Command(_COMMAND_NAME), Description(_COMMAND_DESC), Aliases(new string[] { "roleinfo", "ri" }), Priority(0)]
		public async Task ByName(CommandContext context, [Description(_ARG_ROLENAME_DESC), RemainingText] string roleName)
		{
			roleName = roleName.ToLower();
			DiscordRole roleInQuestion = null;
			// Check if it's the @everyone or @here roles.
			if (roleName == "everyone" || roleName == "@here")
			{
				roleInQuestion = context.Guild.GetRole(context.Guild.Id);
			}
			else
			{
				// Loop through all the other roles if it isn't
				// TODO: Let the user choose which role they want info on if there are duplciate named roles.
				foreach (DiscordRole role in context.Guild.Roles.Values)
				{
					if (role.Name.ToLower() == roleName)
					{
						roleInQuestion = role;
					}
				}
			}
			if (roleInQuestion == null)
			{
				_ = Program.SendMessage(context, $"There was no role called \"{roleName}\""); // No role was found. Inform the user.
			}
			else
			{
				_ = ByPing(context, roleInQuestion); // Role was found, forward it to ByPing.
			}
		}

		[Command(_COMMAND_NAME), Priority(1)]
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
