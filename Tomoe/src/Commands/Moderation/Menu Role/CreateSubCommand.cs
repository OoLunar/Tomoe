using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Tomoe.Commands.Attributes;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    [SlashCommandGroup("menu_role", "Select which roles you want!")]
    public partial class MenuRoleCommand : ApplicationCommandModule
    {
        public readonly Database Database;

        public MenuRoleCommand(Database database) => Database = database;

        [SlashCommand("create", "Creates a new autoreaction on a channel."), Hierarchy(Permissions.ManageChannels | Permissions.ManageMessages)]
        public async Task CreateAsync(InteractionContext context,
            [Option("channel", "Which guild channel to autoreact too.")] DiscordChannel channel,
            [Option("message_text", "The text on the message.")] string messageContent,
            [Option("button_text", "The text on the button.")] string buttonText,
            [Option("role_num", "Which role to add"), ParameterLimit(1, 22)] params DiscordRole[] roles)
        {
            if (channel.Type != ChannelType.Text && channel.Type != ChannelType.News)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {channel.Mention} is not a text channel!"
                });
                return;
            }

            List<MenuRole> reactionRoles = new();
            List<DiscordRole> botUnassignableRoles = new();
            List<DiscordRole> userUnassignableRoles = new();
            foreach (DiscordRole role in roles)
            {
                if (role.Position >= context.Guild.CurrentMember.Hierarchy)
                {
                    botUnassignableRoles.Add(role);
                }
                else if (role.Position >= context.Member.Hierarchy)
                {
                    userUnassignableRoles.Add(role);
                }

                MenuRole reactionRole = new(context.InteractionId.ToString(CultureInfo.InvariantCulture), context.Guild.Id, role.Id);
                reactionRoles.Add(reactionRole);
            }

            if (botUnassignableRoles.Count != 0)
            {
                bool confirm = await context.ConfirmAsync($"Unable to assign the following roles: {string.Join(", ", botUnassignableRoles.Select(role => role.Mention))}. To fix this, move {context.Guild.CurrentMember.Mention}'s highest role above these roles. Continue anyways?");
                if (!confirm)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = "Cancelled."
                    });
                    return;
                }
            }
            else if (userUnassignableRoles.Count != 0 && !context.Member.IsOwner && !context.Member.HasPermission(Permissions.Administrator))
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"You're unable to assign the following roles: {string.Join(", ", userUnassignableRoles.Select(role => role.Mention))}. To fix this, ask an admin to move those roles below your highest role, or have them run the command themselves."
                });
                return;
            }

            PermanentButton permanentButton = new(context.InteractionId.ToString(CultureInfo.InvariantCulture), ButtonType.MenuRole, context.Guild.Id);

            Database.MenuRoles.AddRange(reactionRoles);
            Database.PermanentButtons.Add(permanentButton);
            await Database.SaveChangesAsync();

            DiscordButtonComponent button = new(ButtonStyle.Primary, context.InteractionId + "-1", buttonText);
            DiscordMessageBuilder messageBuilder = new()
            {
                Content = messageContent.Replace("\\n", "\n")
            };
            messageBuilder.AddComponents(button);
            await channel.SendMessageAsync(messageBuilder);

            await context.EditResponseAsync(new()
            {
                Content = "Menu roles created!"
            });
        }
    }
}
