namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("role_info", "Gets general information about a role.")]
        public static async Task RoleInfo(InteractionContext context, [Option("role", "The role to get information on.")] DiscordRole discordRole)
        {
            // TODO: Keep local cache of guild members with roles.
            int totalMemberCount = 0;
            StringBuilder roleMembers = new();
            foreach (DiscordMember member in (await context.Guild.GetAllMembersAsync()).OrderBy(member => member.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                if (member.Roles.Contains(discordRole) || discordRole.Name == "@everyone")
                {
                    totalMemberCount++;
                    if ((roleMembers.Length + $"{member.Mention}, ".Length) < 1024)
                    {
                        roleMembers.Append($"{member.Mention}, ");
                    }
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

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
        }
    }
}