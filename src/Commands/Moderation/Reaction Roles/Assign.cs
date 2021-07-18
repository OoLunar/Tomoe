namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using DSharpPlus.SlashCommands;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public partial class MenuRoles : SlashCommandModule
        {
            public static async Task Assign(DiscordInteraction interaction, DiscordMember member, string buttonId, Database database)
            {
                string id = buttonId.Split('-')[0];
                IEnumerable<DiscordRole> menuRoles = database.MenuRoles.Where(reactionRole => reactionRole.GuildId == interaction.GuildId && reactionRole.ButtonId == id).Select(reactionRole => interaction.Guild.GetRole(reactionRole.RoleId));
                menuRoles = menuRoles.OrderByDescending(role => role.Position);
                IEnumerable<DiscordRole> userRoles = member.Roles.Where(role => menuRoles.Contains(role));
                userRoles = userRoles.OrderByDescending(role => role.Position);
                List<DiscordSelectComponentOption> options = new();

                foreach (DiscordRole role in userRoles)
                {
                    DiscordSelectComponentOption selectOption = new(role.Name, role.Id.ToString(CultureInfo.InvariantCulture), null, true);
                    options.Add(selectOption);
                }

                foreach (DiscordRole role in menuRoles.Except(userRoles))
                {
                    DiscordSelectComponentOption selectOption = new(role.Name, role.Id.ToString(CultureInfo.InvariantCulture), null, false);
                    options.Add(selectOption);
                }

                DiscordInteractionResponseBuilder responseBuilder = new();
                responseBuilder.IsEphemeral = true;
                responseBuilder.Content = "Select your roles!";

                DiscordSelectComponent menu = new(id + '-' + "2", "Select your roles!", options, false, 0, options.Count);
                responseBuilder.AddComponents(menu);
                await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
            }

            public static async Task Assign(ComponentInteractionCreateEventArgs componentInteractionCreateEventArgs, string buttonId, Database database)
            {
                await componentInteractionCreateEventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new()
                {
                    IsEphemeral = true
                });

                string id = buttonId.Split('-')[0];
                IEnumerable<MenuRole> reactionRoles = database.MenuRoles.Where(reactionRole => reactionRole.ButtonId == id && reactionRole.GuildId == componentInteractionCreateEventArgs.Guild.Id).ToList();
                DiscordMember member = await componentInteractionCreateEventArgs.User.Id.GetMember(componentInteractionCreateEventArgs.Guild);
                IEnumerable<ulong> memberRoles = member.Roles.Select(role => role.Id);

                IEnumerable<ulong> roles = componentInteractionCreateEventArgs.Values.Select(value => ulong.Parse(value, CultureInfo.InvariantCulture));
                IEnumerable<ulong> grantRoles = roles.Where(role => !memberRoles.Contains(role));
                IEnumerable<ulong> revokeRoles = reactionRoles.Select(reactionRole => reactionRole.RoleId).Where(role => memberRoles.Contains(role) && !roles.Contains(role));

                foreach (ulong roleId in grantRoles)
                {
                    await member.GrantRoleAsync(componentInteractionCreateEventArgs.Guild.GetRole(roleId), "Select Menu");
                }

                foreach (ulong roleId in revokeRoles)
                {
                    await member.RevokeRoleAsync(componentInteractionCreateEventArgs.Guild.GetRole(roleId), "Select Menu");
                }

                await componentInteractionCreateEventArgs.Interaction.EditOriginalResponseAsync(new()
                {
                    Content = "Assigned roles!"
                });
            }
        }
    }
}