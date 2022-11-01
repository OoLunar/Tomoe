using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    public partial class ConfigCommand : ApplicationCommandModule
    {
        [SlashCommand("mute", "Sets the mute role for the guild."), Hierarchy(Permissions.ManageRoles)]
        public async Task MuteAsync(InteractionContext context, [Option("role", "Which role to set.")] DiscordRole? role = null)
        {
            GuildConfig guildConfig = Database.GuildConfigs.First(guildConfig => guildConfig.Id == context.Guild.Id);
            if (role is null)
            {
                if (guildConfig.MuteRole == 0 || context.Guild.GetRole(guildConfig.MuteRole) is null)
                {
                    bool createRole = await context.ConfirmAsync("Error: The mute role does not exist. Should one be created now?");
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

            await FixRolePermissionsAsync(context.Guild, context.Member, role, CustomEvent.Mute, Database);
            guildConfig.MuteRole = role.Id;
            await Database.SaveChangesAsync();

            await context.EditResponseAsync(new()
            {
                Content = $"The mute role was set to {role.Mention}!"
            });
        }
    }
}
