namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Enums;
    using DSharpPlus.Interactivity.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RoleInfo : BaseCommandModule
    {
        [Command("role_info"), Description("Gets information about a server role."), Aliases("ri"), Priority(0)]
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
            {
                foreach (DiscordRole role in context.Guild.Roles.Values)
                {
                    if (role.Name.ToLowerInvariant() == roleName || role.Name.Contains(roleName))
                    {
                        rolesInQuestion.Add(role);
                    }
                }
            }

            // If no roles were found, try to do an "autocomplete" sort of thing.
            if (rolesInQuestion.Count == 0)
            {
                foreach (DiscordRole role in context.Guild.Roles.Values)
                {
                    if (role.Name.StartsWith(roleName, true, CultureInfo.InvariantCulture))
                    {
                        rolesInQuestion.Add(role);
                    }
                }
            }

            if (rolesInQuestion.Count == 0)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error: There was no role called \"{roleName}\"]")); // No role was found. Inform the user.
            }
            else if (rolesInQuestion.Count == 1)
            {
                await Overload(context, rolesInQuestion[0]);
            }
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
                            if (roleUsers.Length < 992)
                            {
                                roleUsers.Append($"{member.Mention} "); // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.
                            }
                        }
                    }
                    string permissions = role.Permissions.ToPermissionString();
                    if (string.IsNullOrEmpty(permissions.Trim()))
                    {
                        permissions = "None.";
                    }
                    embed.AddField(Formatter.Bold("Members"), roleUsers.Length == 0 ? "None" : roleUsers.ToString());
                    StringBuilder roleInfo = new();
                    roleInfo.Append($"Id: {Formatter.Bold(role.Id.ToString(CultureInfo.InvariantCulture))}\n");
                    roleInfo.Append($"Name: {Formatter.Bold(role.Name.ToString())}\n");
                    roleInfo.Append($"Creation Timestamp: {Formatter.Bold(role.CreationTimestamp.ToString(CultureInfo.InvariantCulture))}\n");
                    roleInfo.Append($"Position: {Formatter.Bold(role.Position.ToString(CultureInfo.InvariantCulture))}\n");
                    roleInfo.Append($"Color: {Formatter.Bold(role.Color.ToString())}");
                    roleInfo.Append($"Mentionable: {Formatter.Bold(role.IsMentionable.ToString())}\n");
                    roleInfo.Append($"Hoisted: {Formatter.Bold(role.IsHoisted.ToString())}\n");
                    roleInfo.Append($"Managed: {Formatter.Bold(role.IsManaged.ToString())}\n");
                    roleInfo.Append($"Permissions: {Formatter.Bold(permissions)}\n");
                    roleInfo.Append($"Member Count: {Formatter.Bold(roleMemberCount.ToString(CultureInfo.InvariantCulture))}");
                    embed.Description = roleInfo.ToString();
                    embeds.Add(new(null, embed));
                    await Task.Delay(50);
                }
                await message.ModifyAsync($"{context.User.Mention}: Found a total of {embeds.Count} roles called {roleName.ToLowerInvariant()}.");
                await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embeds, default, PaginationBehaviour.Ignore);
            }
        }

        [Command("role_info"), Priority(1)]
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
                    if (roleUsers.Length < 992)
                    {
                        roleUsers.Append($"{member.Mention} "); // Max embed length is 1024. Max username length is 32. 1024 - 32 = 992.					}
                    }
                }
            }
            embed.AddField(Formatter.Bold("Members"), roleUsers.Length == 0 ? "None" : roleUsers.ToString());
            string permissions = role.Permissions.ToPermissionString();
            if (string.IsNullOrEmpty(permissions.Trim()))
            {
                permissions = "None.";
            }
            StringBuilder roleInfo = new();
            roleInfo.Append($"Id: {Formatter.Bold(role.Id.ToString(CultureInfo.InvariantCulture))}\n");
            roleInfo.Append($"Name: {Formatter.Bold(role.Name.ToString())}\n");
            roleInfo.Append($"Creation Timestamp: {Formatter.Bold(role.CreationTimestamp.ToString(CultureInfo.InvariantCulture))}\n");
            roleInfo.Append($"Position: {Formatter.Bold(role.Position.ToString(CultureInfo.InvariantCulture))}\n");
            roleInfo.Append($"Color: {Formatter.Bold(role.Color.ToString())}\n");
            roleInfo.Append($"Mentionable: {Formatter.Bold(role.IsMentionable.ToString())}\n");
            roleInfo.Append($"Hoisted: {Formatter.Bold(role.IsHoisted.ToString())}\n");
            roleInfo.Append($"Managed: {Formatter.Bold(role.IsManaged.ToString())}\n");
            roleInfo.Append($"Permissions: {Formatter.Bold(permissions)}\n");
            roleInfo.Append($"Member Count: {Formatter.Bold(roleMemberCount.ToString(CultureInfo.InvariantCulture))}");
            embed.Description = roleInfo.ToString();
            await Program.SendMessage(context, null, embed.Build());
        }
    }
}
