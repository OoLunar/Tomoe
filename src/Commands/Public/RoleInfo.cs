namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using ICSharpCode.Decompiler.CSharp.Syntax;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RoleInfo : SlashCommandModule
    {
        [SlashCommand("role_info", "Gets general information about a role.")]
        public static async Task ByProgram(InteractionContext context, [Option("role", "The role to get information on.")] DiscordRole discordRole)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
            {
                IsEphemeral = true
            });

            int totalMemberCount = 0;
            StringBuilder roleUsers = new();
            foreach (DiscordMember member in context.Guild.Members.Values.OrderBy(member => member.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                if (member.Roles.Contains(discordRole) || discordRole.Name == "@everyone")
                {
                    totalMemberCount++;
                    // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
                    if (roleUsers.Length < 992)
                    {
                        roleUsers.Append($"{member.Mention} ");
                    }
                }
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Role Info for " + discordRole.Name,
                Color = discordRole.Color.Value == 0x000000 ? new DiscordColor("#7b84d1") : discordRole.Color
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
            embedBuilder.AddField("Role Id", discordRole.Id.ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Role Name", discordRole.Name, true);
            embedBuilder.AddField("Role Position", discordRole.Position.ToMetric(), true);
            embedBuilder.AddField("Total Member Count", totalMemberCount.ToMetric(), true);
            string permissions = discordRole.Permissions.ToPermissionString();
            if (string.IsNullOrEmpty(permissions))
            {
                permissions = "No Permissions";
            }
            embedBuilder.AddField("Permissions", permissions, false);
            embedBuilder.AddField("Members", roleUsers.ToString(), false);

            DiscordWebhookBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embedBuilder);
            await context.EditResponseAsync(messageBuilder);
        }
    }
}
