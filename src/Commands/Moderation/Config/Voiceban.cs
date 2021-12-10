namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : ApplicationCommandModule
    {
        public partial class Config : ApplicationCommandModule
        {
            [SlashCommand("voiceban", "Sets the voiceban role for the guild."), Hierarchy(Permissions.ManageRoles)]
            public async Task Voiceban(InteractionContext context, [Option("role", "Which role to set.")] DiscordRole role = null)
            {
                GuildConfig guildConfig = Database.GuildConfigs.First(guildConfig => guildConfig.Id == context.Guild.Id);
                if (role == null)
                {
                    if (guildConfig.VoicebanRole == 0 || context.Guild.GetRole(guildConfig.VoicebanRole) == null)
                    {
                        bool createRole = await context.Confirm("Error: The voiceban role does not exist. Should one be created now?");
                        if (createRole)
                        {
                            role = await context.Guild.CreateRoleAsync("Voiceban", Permissions.None, DiscordColor.VeryDarkGray, false, false, "Used for the voiceban command and config.");
                        }
                        else
                        {
                            await context.EditResponseAsync(new()
                            {
                                Content = "Error: No voiceban role exists, and I did not recieve permission to create it."
                            });
                            return;
                        }
                    }
                    else
                    {
                        role = context.Guild.GetRole(guildConfig.VoicebanRole);
                    }
                }

                await FixRolePermissions(context.Guild, context.Member, role, CustomEvent.Voiceban, Database);
                guildConfig.VoicebanRole = role.Id;
                await Database.SaveChangesAsync();

                await context.EditResponseAsync(new()
                {
                    Content = $"The voiceban role was set to {role.Mention}!"
                });
            }
        }
    }
}