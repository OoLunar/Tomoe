using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Utils;

namespace Tomoe.Commands.Public {
    public class RoleInfo : BaseCommandModule {
        private Logger _logger = new Logger("Commands/Public/RoleInfo");
        [Command("role_info")]
        [Aliases(new string[] { "roleinfo", "ri" })]
        [Description("Gets information about a server role.")]
        [Priority(0)]
        public async Task ByName(CommandContext context, string roleName) {
            DiscordRole roleInQuestion = null;
            // Check if it's the @everyone or @here roles.
            if (roleName.ToLower() == "everyone" || roleName.ToLower() == "here") roleInQuestion = context.Guild.GetRole(context.Guild.Id);
            // Loop through all the other roles if it isn't
            // TODO: Let the user choose which role they want info on if there are duplciate named roles.
            else
                foreach (DiscordRole role in context.Guild.Roles.Values) {
                    if (role.Name.ToLower() == roleName.ToLower()) roleInQuestion = role;
                }
            // No role was found. Inform the user.
            if (roleInQuestion == null) Tomoe.Program.SendMessage(context, $"There was no role called \"{roleName}\"");
            // Role was found, forward it to ByPing.
            else ByPing(context, roleInQuestion);
        }

        [Command("role_info")]
        [Priority(1)]
        public async Task ByPing(CommandContext context, DiscordRole role) {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor { Name = context.User.Username, IconUrl = context.User.AvatarUrl };
            embed.Color = role.Color;
            embed.Timestamp = System.DateTime.UtcNow;
            embed.Title = $"Role Info for **{role.Name}**";
            int roleMemberCount = 0;
            string roleUsers = "";
            foreach (DiscordMember member in context.Guild.Members.Values) {
                if (member.Roles.Contains(role) || role.Name == "@everyone") {
                    roleMemberCount++;
                    if (roleUsers.Length <= 991) roleUsers += $"{member.Mention} ";
                }
            }
            embed.AddField("**Members**", string.IsNullOrEmpty(roleUsers) ? "None." : roleUsers);
            embed.Description = $"ID: **{role.Id}**\nName: **{role.Name}**\nCreation: **{role.CreationTimestamp}**\nPosition: **{role.Position}**\nColor: **{role.Color}**\nMentionable: **{role.IsMentionable}**\nHoisted: **{role.IsHoisted}**\nManaged: **{role.IsManaged}**\nPermissions: **{role.Permissions.ToPermissionString()}**\nMembers: **{roleMemberCount}**";
            Tomoe.Program.SendMessage(context, embed.Build());
        }
    }
}