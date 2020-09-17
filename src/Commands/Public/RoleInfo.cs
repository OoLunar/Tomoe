using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace Tomoe.Commands.Public {
    public class RoleInfo : InteractiveBase {

        /// <summary>
        /// Sends a Discord message, then edits the message with how long it took to send in milliseconds.
        /// <code>
        /// >>ping
        /// </code>
        /// </summary>
        [Command("role_info", RunMode = RunMode.Async)]
        [Summary("[Gets the permissions of a role, the role color, who has the role and a count of who has the role.](https://github.com/OoLunar/Tomoe/blob/master/docs/public/role_info.md)")]
        [Remarks("Public")]
        public async Task ByName(string roleName) {
            IRole roleInQuestion = null;
            if (roleName == "everyone" || roleName == "here") { await ByMention(Context.Guild.GetRole(Context.Guild.Id)); return; };
            foreach (IRole role in Context.Guild.Roles)
                if (role.Name == roleName) roleInQuestion = role;
            if (roleInQuestion == null) await ReplyAsync($"There was no role found for {roleName}");
            else await ByMention(roleInQuestion);
        }

        [Command("role_info", RunMode = RunMode.Async)]
        public async Task ByID(ulong roleID) {
            IRole roleInQuestion = Context.Guild.GetRole(roleID);
            if (roleInQuestion == null) await ReplyAsync($"There was no role found for {roleID}");
            else await ByMention(roleInQuestion);
        }

        [Command("role_info", RunMode = RunMode.Async)]
        public async Task ByMention(IRole roleInQuestion) {
            string perms = "";
            int roleMemberCount = 0;
            string roleUsers = "";
            foreach (SocketGuildUser member in (roleInQuestion as SocketRole).Members) {
                roleMemberCount++;
                if (roleUsers.Length <= 1800) roleUsers += $"{member.Mention} ";
            }
            roleInQuestion.Permissions.ToList().ForEach(perm => perms += $"`{perm.ToString()}`, ");
            EmbedBuilder embed = new EmbedBuilder();
            embed.Description = $"ID: **{roleInQuestion.Id}**\nName: **{roleInQuestion.Name}**\nCreation: **{roleInQuestion.CreatedAt.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'")}**\nPosition: **{roleInQuestion.Position}**\nColor: **{roleInQuestion.Color}**\nMentionable: **{roleInQuestion.IsMentionable}**\nHoisted: **{roleInQuestion.IsHoisted}**\nManaged: **{roleInQuestion.IsManaged}**\nPermissions: **{(string.IsNullOrWhiteSpace(perms) ? "None" : perms)}**\nMembers: **{roleMemberCount}**";
            embed.AddField("Members:", (string.IsNullOrWhiteSpace(roleUsers) ? "None" : roleUsers));
            embed.Color = roleInQuestion.Color == Color.Default ? Color.Default : roleInQuestion.Color;
            await ReplyAsync(null, false, embed.Build());
        }
    }
}