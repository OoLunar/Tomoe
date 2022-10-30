using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Db;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        public Database Database { private get; set; } = null!;

        [SlashCommand("role_info", "Gets general information about a role.")]
        public Task RoleInfoAsync(InteractionContext context, [Option("role", "The role to get information on.")] DiscordRole discordRole)
        {
            int totalMemberCount = 0;
            StringBuilder roleMembers = new();
            if (discordRole.Id == context.Guild.Id) // everyone role
            {
                totalMemberCount = context.Guild.MemberCount;
                roleMembers.Append("Everyone");
            }
            else
            {
                foreach (GuildMember member in Database.GuildMembers.Where(guildMember => guildMember.GuildId == context.Guild.Id && guildMember.Roles.Contains(discordRole.Id)).AsEnumerable().OrderBy(member => context.Guild.Members.ContainsKey(member.UserId) ? context.Guild.Members[member.UserId].DisplayName : string.Empty))
                {
                    string memberMention = $"<@{member.UserId}>, ";
                    int stringBuilderLength = roleMembers.Append(memberMention).Length;
                    if (stringBuilderLength > 2000)
                    {
                        roleMembers.Remove(stringBuilderLength - memberMention.Length, memberMention.Length);
                        break;
                    }

                    totalMemberCount++;
                }
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Role Info For " + discordRole.Name,
                Color = discordRole.Color.Value == 0 ? new DiscordColor("#7b84d1") : discordRole.Color
            };

            if (context.Guild.IconUrl != null)
            {
                embedBuilder.WithThumbnail(context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"));
            }

            embedBuilder.AddField("Color", discordRole.Color.ToString(), true);
            embedBuilder.AddField("Created At", discordRole.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
            embedBuilder.AddField("Hoisted", discordRole.IsHoisted.ToString(), true);
            embedBuilder.AddField("Is Managed", discordRole.IsManaged.ToString(), true);
            embedBuilder.AddField("Is Mentionable", discordRole.IsMentionable.ToString(), true);
            embedBuilder.AddField("Role Id", Formatter.InlineCode(discordRole.Id.ToString(CultureInfo.InvariantCulture)), true);
            embedBuilder.AddField("Role Name", discordRole.Name, true);
            embedBuilder.AddField("Role Position", discordRole.Position.ToMetric(), true);
            embedBuilder.AddField("Total Member Count", totalMemberCount.ToMetric(), true);
            embedBuilder.AddField("Permissions", discordRole.Permissions.ToPermissionString());
            embedBuilder.AddField("Members", roleMembers.Length == 0 ? "None." : roleMembers.ToString());

            return context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
        }
    }
}
