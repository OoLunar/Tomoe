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
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class RoleInfo : BaseCommandModule
    {
        [Command("role_info"), Description("Gets information about a server role."), RequireGuild]
        public async Task RoleInfoAsync(CommandContext context, [Description("Which role to get info on.")] DiscordRole discordRole)
        {
            List<Page> pages = new();
            int totalMemberCount = 0;
            StringBuilder roleUsers = new();
            foreach (DiscordMember member in (await context.Guild.GetAllMembersAsync()).OrderBy(member => member.DisplayName, StringComparer.CurrentCultureIgnoreCase))
            {
                if (member.Roles.Contains(discordRole) || discordRole.Name == "@everyone")
                {
                    totalMemberCount++;

                    // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
                    if (roleUsers.Length < 992)
                    {
                        roleUsers.Append(CultureInfo.InvariantCulture, $"{member.Mention} ");
                    }
                }
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Role Info for {discordRole.Name}",
                Author = new()
                {
                    Name = context.Member!.DisplayName,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Color = discordRole.Color.Value == 0x000000 ? Optional.FromNoValue<DiscordColor>() : discordRole.Color
            };
            embedBuilder.AddField("Color", discordRole.Color.ToString(), true);
            embedBuilder.AddField("Created At", discordRole.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
            embedBuilder.AddField("Hoisted", discordRole.IsHoisted.ToString(), true);
            embedBuilder.AddField("Is Managed", discordRole.IsManaged.ToString(), true);
            embedBuilder.AddField("Is Mentionable", discordRole.IsMentionable.ToString(), true);
            embedBuilder.AddField("Role Id", discordRole.Id.ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Role Name", discordRole.Name, true);
            embedBuilder.AddField("Role Position", discordRole.Position.ToMetric(), true);
            embedBuilder.AddField("Role Member Count", totalMemberCount.ToMetric(), true);
            embedBuilder.AddField("Permissions", discordRole.Permissions == Permissions.None ? "No permissions." : discordRole.Permissions.ToPermissionString() + ".", false);
            embedBuilder.AddField("Members", roleUsers.ToString(), false);

            await context.RespondAsync(embedBuilder);
        }
    }
}
