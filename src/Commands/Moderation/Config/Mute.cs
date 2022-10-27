namespace Tomoe.Commands
{
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : ApplicationCommandModule
    {
        public partial class Config : ApplicationCommandModule
        {
            [SlashCommand("mute", "Sets the mute role for the guild."), Hierarchy(Permissions.ManageRoles)]
            public async Task Mute(InteractionContext context, [Option("role", "Which role to set.")] DiscordRole role = null)
            {
                GuildConfig guildConfig = Database.GuildConfigs.First(guildConfig => guildConfig.Id == context.Guild.Id);
                if (role == null)
                {
                    if (guildConfig.MuteRole == 0 || context.Guild.GetRole(guildConfig.MuteRole) == null)
                    {
                        bool createRole = await context.Confirm("Error: The mute role does not exist. Should one be created now?");
                        if (createRole)
                        {
                            role = await context.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.VeryDarkGray, false, false, "Used for the mute command and config.");
                        }
                        else
                        {
                            await context.EditResponseAsync(new()
                            {
                                Content = "Error: No mute role exists, and I did not recieve permission to create it."
                            });
                            return;
                        }
                    }
                    else
                    {
                        role = context.Guild.GetRole(guildConfig.MuteRole);
                    }
                }

                await FixRolePermissions(context.Guild, context.Member, role, CustomEvent.Mute, Database);
                guildConfig.MuteRole = role.Id;
                await Database.SaveChangesAsync();

                await context.EditResponseAsync(new()
                {
                    Content = $"The mute role was set to {role.Mention}!"
                });
            }
        }
    }
}
