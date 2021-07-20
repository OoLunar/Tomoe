namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommandGroup("menu_role", "Select which roles you want!")]
        public partial class MenuRoles : SlashCommandModule
        {
            public Database Database { private get; set; }

            [SlashCommand("create", "Creates a new autoreaction on a channel."), Hierarchy(Permissions.ManageChannels | Permissions.ManageMessages)]
            public async Task Create(InteractionContext context, [Option("channel", "Which guild channel to autoreact too.")] DiscordChannel channel,
                [Option("role1", "Which role to add as a menu role.")] DiscordRole role1,
                [Option("role2", "Which role to add as a menu role.")] DiscordRole role2 = null,
                [Option("role3", "Which role to add as a menu role.")] DiscordRole role3 = null,
                [Option("role4", "Which role to add as a menu role.")] DiscordRole role4 = null,
                [Option("role5", "Which role to add as a menu role.")] DiscordRole role5 = null,
                [Option("role6", "Which role to add as a menu role.")] DiscordRole role6 = null,
                [Option("role7", "Which role to add as a menu role.")] DiscordRole role7 = null,
                [Option("role8", "Which role to add as a menu role.")] DiscordRole role8 = null,
                [Option("role9", "Which role to add as a menu role.")] DiscordRole role9 = null,
                [Option("role10", "Which role to add as a menu role.")] DiscordRole role10 = null,
                [Option("role11", "Which role to add as a menu role.")] DiscordRole role11 = null,
                [Option("role12", "Which role to add as a menu role.")] DiscordRole role12 = null,
                [Option("role13", "Which role to add as a menu role.")] DiscordRole role13 = null,
                [Option("role14", "Which role to add as a menu role.")] DiscordRole role14 = null,
                [Option("role15", "Which role to add as a menu role.")] DiscordRole role15 = null,
                [Option("role16", "Which role to add as a menu role.")] DiscordRole role16 = null,
                [Option("role17", "Which role to add as a menu role.")] DiscordRole role17 = null,
                [Option("role18", "Which role to add as a menu role.")] DiscordRole role18 = null,
                [Option("role19", "Which role to add as a menu role.")] DiscordRole role19 = null,
                [Option("role20", "Which role to add as a menu role.")] DiscordRole role20 = null,
                [Option("role21", "Which role to add as a menu role.")] DiscordRole role21 = null,
                [Option("role22", "Which role to add as a menu role.")] DiscordRole role22 = null,
                [Option("role23", "Which role to add as a menu role.")] DiscordRole role23 = null,
                [Option("role24", "Which role to add as a menu role.")] DiscordRole role24 = null)
            {
                if (channel.Type != ChannelType.Text && channel.Type != ChannelType.News)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: {channel.Mention} is not a text channel!"
                    });
                    return;
                }


                DiscordButtonComponent button = new(ButtonStyle.Primary, context.InteractionId + "-1", "Click Me!");
                List<MenuRole> reactionRoles = new();
                IEnumerable<DiscordRole> roles = new[] { role1, role2, role3, role4, role5, role6, role7, role8, role9, role10, role11, role12, role13, role14, role15, role16, role17, role18, role19, role20, role21, role22, role23, role24 }.Where(role => role != null);
                foreach (DiscordRole role in roles)
                {
                    MenuRole reactionRole = new()
                    {
                        GuildId = context.Guild.Id,
                        RoleId = role.Id,
                        ButtonId = context.InteractionId.ToString(CultureInfo.InvariantCulture)
                    };
                    reactionRoles.Add(reactionRole);
                }

                PermanentButton permanentButton = new()
                {
                    GuildId = context.Guild.Id,
                    ButtonId = context.InteractionId.ToString(CultureInfo.InvariantCulture),
                    ButtonType = ButtonType.MenuRole
                };

                Database.MenuRoles.AddRange(reactionRoles);
                Database.PermanentButtons.Add(permanentButton);
                await Database.SaveChangesAsync();

                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.Content = "Select Your Roles!";
                messageBuilder.AddComponents(button);
                await context.Channel.SendMessageAsync(messageBuilder);

                await context.EditResponseAsync(new()
                {
                    Content = "Menu roles created!"
                });
            }
        }
    }
}