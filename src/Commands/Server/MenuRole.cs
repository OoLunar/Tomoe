using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;
using Tomoe.Models;

namespace Tomoe.Commands.Server
{
    [Group("menu_role")]
    public class MenuRole : BaseCommandModule
    {
        public DatabaseContext Database { private get; init; } = null!;

        [Command("add")]
        public async Task AddAsync(CommandContext context, DiscordChannel channel, string messageText = "Select your custom roles!", string buttonText = "Menu Roles!", [RemainingText] params DiscordRole[] roles)
        {
            if (!context.Member.Permissions.HasPermission(Permissions.ManageRoles))
            {
                await context.RespondAsync($"[Error]: You're missing the {Permissions.ManageRoles.Humanize()} permission!");
                return;
            }
            else if (!context.Member.PermissionsIn(channel).HasPermission(Permissions.SendMessages))
            {
                await context.RespondAsync($"[Error]: You don't have the {Permissions.SendMessages.Humanize()} permission in the {channel.Mention} channel.");
                return;
            }
            else if (!context.Guild.CurrentMember.Permissions.HasPermission(Permissions.ManageRoles))
            {
                await context.RespondAsync($"[Error]: I'm missing the {Permissions.ManageRoles.Humanize()} permission!");
                return;
            }
            else if (!context.Guild.CurrentMember.PermissionsIn(channel).HasPermission(Permissions.SendMessages))
            {
                await context.RespondAsync($"[Error]: I don't have the {Permissions.SendMessages.Humanize()} permission in the {channel.Mention} channel.");
                return;
            }
            else if (roles.Length == 0)
            {
                await context.RespondAsync("[Error]: You need to specify at least 1 role to add to the menu.");
                return;
            }
            else if (roles.Length > 24)
            {
                await context.RespondAsync("[Error]: You can only add up to 24 roles to the menu.");
                return;
            }
            else if (roles.Any(x => x.IsManaged))
            {
                await context.RespondAsync("[Error]: You can't add bot/managed/integrated roles to the menu. Discord manages those, not us.");
                return;
            }
            else if (roles.Any(x => x.Id == context.Guild.Id))
            {
                await context.RespondAsync($"[Error]: You can't add the {context.Guild.EveryoneRole.Mention} role to the menu. Everyone already has that role.");
                return;
            }

            IEnumerable<DiscordRole> hierarchyRoles = roles.Where(x => x.Position >= context.Member.Hierarchy);
            if (hierarchyRoles.Any())
            {
                await context.RespondAsync($"[Error]: The following roles are higher than your top role: {hierarchyRoles.Select(x => x.Mention).Humanize()}. If you wish to add these roles, please contact a server administrator.");
                return;
            }

            DiscordMessageBuilder messageBuilder = new();
            hierarchyRoles = roles.Where(x => x.Position >= context.Guild.CurrentMember.Hierarchy);
            if (hierarchyRoles.Any())
            {
                string? roleText = context.Guild.CurrentMember.Roles.FirstOrDefault(x => (x.Tags?.BotId.HasValue ?? false) && x.Tags.BotId.Value == context.Guild.CurrentMember.Id)?.Mention;
                roleText = roleText == null ? "a role that I have" : $"my role ({roleText})";

                messageBuilder.Content = $"[Error]: The following roles are higher than my top role: {hierarchyRoles.Select(x => x.Mention).Humanize()}. In order for me to assign these roles, I'll need a server administrator to move {roleText} above the {hierarchyRoles.OrderByDescending(x => x.Position).First().Mention} role.";
                messageBuilder.WithAllowedMentions(Mentions.None);
                await context.RespondAsync(messageBuilder);
                return;
            }

            Guid buttonId = Guid.NewGuid();
            List<MenuRoleModel> menuRoles = new();
            foreach (DiscordRole role in roles)
            {
                menuRoles.Add(new MenuRoleModel()
                {
                    ButtonId = buttonId.ToString(),
                    GuildId = context.Guild.Id,
                    RoleId = role.Id,
                });
            }
            Database.MenuRoles.AddRange(menuRoles);
            await Database.SaveChangesAsync();

            IArgumentConverter<DiscordEmoji> emojiArgumentConverter = new DiscordEmojiConverter();
            DiscordButtonComponent button;
            string[] splitList = buttonText.Split(" ");
            button = !buttonText.StartsWith(" ", true, CultureInfo.InvariantCulture) // If the button text starts with a space, treat the emoji as text. An example of this is " :smile: Hello" or " :gear:"
                    && (await emojiArgumentConverter.ConvertAsync(splitList[0], context)).IsDefined(out DiscordEmoji? emoji)
                    && emoji != null
                        ? (new(ButtonStyle.Primary, $"menurole\v{buttonId}\vselect", string.Join(' ', splitList.Skip(1)), false, new DiscordComponentEmoji(emoji)))
                        : (new(ButtonStyle.Primary, $"menurole\v{buttonId}\vselect", buttonText));
            messageBuilder.AddComponents(button);
            messageBuilder.Content = messageText;
            await channel.SendMessageAsync(messageBuilder);

            await context.RespondAsync("Menu Role created!");
        }

        [Command("add")]
        public async Task AddAsync(CommandContext context, DiscordMessage message, [RemainingText] params DiscordRole[] roles)
        {
            if (!context.Member.Permissions.HasPermission(Permissions.ManageRoles))
            {
                await context.RespondAsync($"[Error]: You're missing the {Permissions.ManageRoles.Humanize()} permission!");
                return;
            }
            else if (!context.Member.PermissionsIn(message.Channel).HasPermission(Permissions.SendMessages))
            {
                await context.RespondAsync($"[Error]: You don't have the {Permissions.SendMessages.Humanize()} permission in the {message.Channel.Mention} channel.");
                return;
            }
            else if (!context.Guild.CurrentMember.Permissions.HasPermission(Permissions.ManageRoles))
            {
                await context.RespondAsync($"[Error]: I'm missing the {Permissions.ManageRoles.Humanize()} permission!");
                return;
            }
            else if (roles.Length == 0)
            {
                await context.RespondAsync("[Error]: You need to specify at least 1 role to add to the menu.");
                return;
            }
            else if (roles.Length > 24)
            {
                await context.RespondAsync("[Error]: You can only add up to 24 roles to the menu.");
                return;
            }
            else if (roles.Any(x => x.IsManaged))
            {
                await context.RespondAsync("[Error]: You can't add bot/managed/integrated roles to the menu. Discord manages those, not us.");
                return;
            }
            else if (roles.Any(x => x.Id == context.Guild.Id))
            {
                await context.RespondAsync($"[Error]: You can't add the {context.Guild.EveryoneRole.Mention} role to the menu. Everyone already has that role.");
                return;
            }

            IEnumerable<DiscordRole> hierarchyRoles = roles.Where(x => x.Position >= context.Member.Hierarchy);
            if (hierarchyRoles.Any())
            {
                await context.RespondAsync($"[Error]: The following roles are higher than your top role: {hierarchyRoles.Select(x => x.Mention).Humanize()}. If you wish to add these roles, please contact a server administrator.");
                return;
            }

            // Search the message for button components, then split by \v, then check if the custom id follows the menu role format.
            string buttonId;
            if (message.Components.Count != 0)
            {
                if (message.Components.First().Components.Count != 0)
                {
                    string[] idParts = message.Components.First().Components.First().CustomId.Split('\v');
                    if (idParts.Length != 3 || idParts[0] != "menurole")
                    {
                        await context.RespondAsync("[Error]: This is not a menu role message.");
                        return;
                    }

                    if (!Guid.TryParse(idParts[1], out Guid buttonGuid))
                    {
                        await context.RespondAsync("[Error]: This is not a menu role message.");
                        return;
                    }
                    buttonId = buttonGuid.ToString();
                }
                else
                {
                    await context.RespondAsync("[Error]: This is not a menu role message.");
                    return;
                }
            }
            else
            {
                await context.RespondAsync("[Error]: This is not a menu role message.");
                return;
            }

            IEnumerable<MenuRoleModel> menuRoles = Database.MenuRoles.Where(x => x.ButtonId == buttonId && x.GuildId == context.Guild.Id);
            if (!menuRoles.Any())
            {
                await context.RespondAsync("[Error]: Unable to find the Menu Role with this button id!");
                return;
            }
            else if (menuRoles.Count() >= 24)
            {
                await context.RespondAsync($"[Error]: You can only add up to 24 roles to the menu. The current menu has {menuRoles.Count()} roles.");
                return;
            }

            List<MenuRoleModel> newMenuRoles = new();
            foreach (DiscordRole role in roles)
            {
                if (!menuRoles.Any(x => x.RoleId == role.Id)) // Filters out duplicates
                {
                    newMenuRoles.Add(new MenuRoleModel()
                    {
                        ButtonId = buttonId.ToString(),
                        GuildId = context.Guild.Id,
                        RoleId = role.Id,
                    });
                }
            }
            Database.MenuRoles.AddRange(newMenuRoles);
            await Database.SaveChangesAsync();

            await context.RespondAsync("Menu Role updated! The following roles were added: " + newMenuRoles.Select(x => $"<@&{x.RoleId}>").Humanize());
        }

        [Command("remove")]
        public async Task RemoveAsync(CommandContext context, DiscordMessage message, [RemainingText] params DiscordRole[] roles)
        {
            if (!context.Member.Permissions.HasPermission(Permissions.ManageRoles))
            {
                await context.RespondAsync($"[Error]: You're missing the {Permissions.ManageRoles.Humanize()} permission!");
                return;
            }
            else if (!context.Member.PermissionsIn(message.Channel).HasPermission(Permissions.SendMessages))
            {
                await context.RespondAsync($"[Error]: You don't have the {Permissions.SendMessages.Humanize()} permission in the {message.Channel.Mention} channel.");
                return;
            }
            else if (!context.Guild.CurrentMember.Permissions.HasPermission(Permissions.ManageRoles))
            {
                await context.RespondAsync($"[Error]: I'm missing the {Permissions.ManageRoles.Humanize()} permission!");
                return;
            }
            else if (roles.Length == 0)
            {
                await context.RespondAsync("[Error]: You need to specify at least 1 role to add to the menu.");
                return;
            }
            else if (roles.Length > 24)
            {
                await context.RespondAsync("[Error]: You can only add up to 24 roles to the menu.");
                return;
            }
            else if (roles.Any(x => x.IsManaged))
            {
                await context.RespondAsync("[Error]: You can't add bot/managed/integrated roles to the menu. Discord manages those, not us.");
                return;
            }
            else if (roles.Any(x => x.Id == context.Guild.Id))
            {
                await context.RespondAsync($"[Error]: You can't add the {context.Guild.EveryoneRole.Mention} role to the menu. Everyone already has that role.");
                return;
            }

            IEnumerable<DiscordRole> hierarchyRoles = roles.Where(x => x.Position >= context.Member.Hierarchy);
            if (hierarchyRoles.Any())
            {
                await context.RespondAsync($"[Error]: The following roles are higher than your top role: {hierarchyRoles.Select(x => x.Mention).Humanize()}. If you wish to add these roles, please contact a server administrator.");
                return;
            }

            // Search the message for button components, then split by \v, then check if the custom id follows the menu role format.
            string buttonId;
            if (message.Components.Count != 0)
            {
                if (message.Components.First().Components.Count != 0)
                {
                    string[] idParts = message.Components.First().Components.First().CustomId.Split('\v');
                    if (idParts.Length != 3 || idParts[0] != "menurole")
                    {
                        await context.RespondAsync("[Error]: This is not a menu role message.");
                        return;
                    }

                    if (!Guid.TryParse(idParts[1], out Guid buttonGuid))
                    {
                        await context.RespondAsync("[Error]: This is not a menu role message.");
                        return;
                    }
                    buttonId = buttonGuid.ToString();
                }
                else
                {
                    await context.RespondAsync("[Error]: This is not a menu role message.");
                    return;
                }
            }
            else
            {
                await context.RespondAsync("[Error]: This is not a menu role message.");
                return;
            }

            IEnumerable<MenuRoleModel> menuRoles = Database.MenuRoles.Where(x => x.ButtonId == buttonId && x.GuildId == context.Guild.Id);
            if (!menuRoles.Any())
            {
                await context.RespondAsync("[Error]: Unable to find the Menu Role with this button id!");
                return;
            }
            else if (menuRoles.Count() >= 24)
            {
                await context.RespondAsync($"[Error]: You can only add up to 24 roles to the menu. The current menu has {menuRoles.Count()} roles.");
                return;
            }

            IEnumerable<ulong> roleIds = roles.Select(x => x.Id);
            List<MenuRoleModel> newMenuRoles = new(Database.MenuRoles.Where(x => x.ButtonId == buttonId && roleIds.Contains(x.RoleId) && x.GuildId == context.Guild.Id));
            Database.MenuRoles.RemoveRange(newMenuRoles);
            await Database.SaveChangesAsync();

            await context.RespondAsync("Menu Role updated! The following roles were removed: " + newMenuRoles.Select(x => $"<@&{x.RoleId}>").Humanize());
        }
    }
}
