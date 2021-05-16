namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;
    using Humanizer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RoleInfo : BaseCommandModule
    {
        [Command("role_info"), Description("Gets information about a server role.")]
        public async Task Overload(CommandContext context, DiscordRole discordRole) => await Program.SendMessage(context, null, ByProgram(context, discordRole)[0].Embed);

        [Command("role_info")]
        public async Task Overload(CommandContext context, [RemainingText] string discordRoleName)
        {
            discordRoleName = discordRoleName.ToLowerInvariant();
            if (discordRoleName is "everyone" or "here" or "@here")
            {
                discordRoleName = "@everyone";
            }

            Page[] pages = ByProgram(context, context.Guild.Roles.Values.Where((DiscordRole discordGuildRole) => discordGuildRole.Name.ToLowerInvariant() == discordRoleName.ToLowerInvariant()).ToArray());
            if (pages.Length == 0)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Role \"{discordRoleName}\" not found!"));
            }
            else if (pages.Length == 1)
            {
                await Program.SendMessage(context, null, pages[0].Embed);
            }
            else
            {
                await context.Client.GetInteractivity().SendPaginatedMessageAsync(context.Channel, context.User, pages);
            }
        }

        public static Page[] ByProgram(CommandContext context, params DiscordRole[] discordRoles)
        {
            List<Page> pages = new();
            foreach (DiscordRole discordRole in discordRoles)
            {
                int totalMemberCount = 0;
                StringBuilder roleUsers = new();
                foreach (DiscordMember member in context.Guild.Members.Values.OrderBy(member => member.DisplayName, StringComparer.CurrentCultureIgnoreCase))
                {
                    if (member.Roles.Contains(discordRole) || discordRole.Name == "@everyone")
                    {
                        totalMemberCount++;
                        if (roleUsers.Length < 992)
                        {
                            roleUsers.Append($"{member.Mention} "); // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
                        }
                    }
                }

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Role Info for {discordRole.Name}");
                embedBuilder.Color = discordRole.Color.Value == 0x000000 ? DiscordColor.Blurple : discordRole.Color;
                embedBuilder.AddField("Color", discordRole.Color.ToString(), true);
                embedBuilder.AddField("Created At", discordRole.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
                embedBuilder.AddField("Hoisted", discordRole.IsHoisted.ToString(), true);
                embedBuilder.AddField("Is Managed", discordRole.IsManaged.ToString(), true);
                embedBuilder.AddField("Is Mentionable", discordRole.IsMentionable.ToString(), true);
                embedBuilder.AddField("Role Id", discordRole.Id.ToString(), true);
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

                Page page = new();
                page.Embed = embedBuilder.Build();
                pages.Add(page);
            }

            return pages.ToArray();
        }
    }
}
