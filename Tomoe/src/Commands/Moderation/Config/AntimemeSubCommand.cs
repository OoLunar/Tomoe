using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    public sealed partial class ConfigCommand : ApplicationCommandModule
    {
        [SlashCommand("antimeme", "Sets the antimeme role for the guild."), Hierarchy(Permissions.ManageRoles)]
        public async Task AntimemeAsync(InteractionContext context, [Option("role", "Which role to set.")] DiscordRole? role = null)
        {
            GuildConfig guildConfig = Database.GuildConfigs.First(guildConfig => guildConfig.Id == context.Guild.Id);
            if (role is null)
            {
                if (guildConfig.AntimemeRole == 0 || context.Guild.GetRole(guildConfig.AntimemeRole) is null)
                {
                    bool createRole = await context.ConfirmAsync("Error: The antimeme role does not exist. Should one be created now?");
                    if (createRole)
                    {
                        role = await context.Guild.CreateRoleAsync("Antimemed", Permissions.None, DiscordColor.VeryDarkGray, false, false, "Used for the antimeme command and config.");
                    }
                    else
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = "Error: No antimeme role exists, and I did not recieve permission to create it."
                        });
                        return;
                    }
                }
                else
                {
                    role = context.Guild.GetRole(guildConfig.AntimemeRole);
                }
            }

            await FixRolePermissionsAsync(context.Guild, context.Member, role, CustomEvent.Antimeme, Database);
            guildConfig.AntimemeRole = role.Id;
            await Database.SaveChangesAsync();

            await context.EditResponseAsync(new()
            {
                Content = $"The antimeme role was set to {role.Mention}!"
            });
        }
    }
}
