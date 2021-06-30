namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation : SlashCommandModule
    {
        public partial class Config : SlashCommandModule
        {
            [SlashCommand("antimeme", "Sets the antimeme role for the guild.")]
            public async Task Antimeme(InteractionContext context, [Option("role", "Which role to set.")] DiscordRole role = null)
            {
                GuildConfig guildConfig = Database.GuildConfigs.First(guildConfig => guildConfig.Id == context.Guild.Id);
                bool prompted = false;
                if (role == null)
                {
                    if (guildConfig.AntimemeRole == 0 || context.Guild.GetRole(guildConfig.AntimemeRole) == null)
                    {
                        bool createRole = await context.Confirm("Error: The Antimeme role is not set in the guild config, and you did not provide one in the command. Should one be created now?");
                        if (!createRole)
                        {
                            await context.EditResponseAsync(new()
                            {
                                Content = "Error: No antimeme role exists, and I did not recieve permission to create it."
                            });
                            return;
                        }
                        else
                        {
                            role ??= await context.Guild.CreateRoleAsync("Antimemed", Permissions.None, DiscordColor.VeryDarkGray, false, false, "Used for the Antimeme command and config.");
                            prompted = true;
                        }
                    }
                    else
                    {
                        role = context.Guild.GetRole(guildConfig.AntimemeRole);
                    }
                }

                if (!prompted)
                {
                    await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new());
                }

                string auditLogReason = "Configuring permissions for antimeme role. Preventing role from reacting to messages, embedding links and uploading files. In voice channels, preventing role from streaming and forcing push-to-talk.";

                foreach (DiscordChannel channel in context.Guild.Channels.Values)
                {
                    if (channel.Type == ChannelType.Category)
                    {
                        await channel.AddOverwriteAsync(role, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection, auditLogReason);
                    }
                    else if (channel.Type == ChannelType.Voice)
                    {
                        await channel.AddOverwriteAsync(role, Permissions.None, Permissions.Stream | Permissions.UseVoiceDetection, auditLogReason);
                    }
                    else
                    {
                        await channel.AddOverwriteAsync(role, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis, auditLogReason);
                    }
                }

                guildConfig.AntimemeRole = role.Id;
                await Database.SaveChangesAsync();

                await context.EditResponseAsync(new()
                {
                    Content = $"The antimeme role was set to {role.Mention}!"
                });
            }
        }
    }
}