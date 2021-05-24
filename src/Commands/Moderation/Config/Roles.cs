namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using Tomoe.Utils.Types;
    using static Tomoe.Api.Moderation;

    public partial class Config : BaseCommandModule
    {
        [Command("antimeme"), RequireBotPermissions(Permissions.ManageRoles | Permissions.ManageChannels), RequireUserPermissions(Permissions.ManageMessages), Description("Creates or assigns the `Antimeme` role. The antimeme role prevents reacting to messages, embedding links, uploading files, streaming and forces push-to-talk. The intention of this role is to prevent abuse of Discord's rich messaging features, or when someone is being really annoying by conversating with every known method except through words.")]
        public async Task Antimeme(CommandContext context, DiscordRole antimemeRole) => await SetRole(context, RoleAction.Antimeme, antimemeRole);
        [Command("antimeme")]
        public async Task Antimeme(CommandContext context) => await SetRole(context, RoleAction.Antimeme, null);

        [Command("mute"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles | Permissions.ManageChannels), RequireUserPermissions(Permissions.ManageMessages), Aliases("silence", "shut", "zip"), Description("Creates or assigns the `Mute` role. The mute role prevents sending messages, reacting to messages and speaking in voice channels.")]
        public async Task Mute(CommandContext context, DiscordRole muteRole) => await SetRole(context, RoleAction.Mute, muteRole);
        [Command("mute")]
        public async Task Mute(CommandContext context) => await SetRole(context, RoleAction.Mute, null);

        [Command("voiceban"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles | Permissions.ManageChannels), RequireUserPermissions(Permissions.ManageMessages), Aliases("vb"), Description("Creates or assigns the `Voiceban` role. The voiceban role prevents connecting to voice channels.")]
        public async Task Voiceban(CommandContext context, DiscordRole voicebanRole) => await SetRole(context, RoleAction.Voiceban, voicebanRole);
        [Command("voiceban")]
        public async Task Voiceban(CommandContext context) => await SetRole(context, RoleAction.Voiceban, null);

        public async Task SetRole(CommandContext context, RoleAction roleAction, DiscordRole role, bool prompt = true)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            bool previousRoleSet = false;
            RoleAction previousRoleType = RoleAction.None;
            switch (roleAction)
            {
                case RoleAction.Antimeme:
                    if (guildConfig.AntimemeRole.GetRole(context.Guild) != null && prompt)
                    {
                        previousRoleSet = true;
                        previousRoleType = RoleAction.Antimeme;
                        break;
                    }
                    else if (role == null)
                    {
                        role = await context.Guild.CreateRoleAsync("Antimemed", Permissions.None, DiscordColor.Gray, false, false, "Creating antimeme role...");
                    }
                    guildConfig.AntimemeRole = role.Id;
                    break;
                case RoleAction.Mute:
                    if (guildConfig.MuteRole.GetRole(context.Guild) != null && prompt)
                    {
                        previousRoleSet = true;
                        previousRoleType = RoleAction.Mute;
                        break;
                    }
                    else if (role == null)
                    {
                        role = await context.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.Gray, false, false, "Creating mute role...");
                    }
                    guildConfig.MuteRole = role.Id;
                    break;
                case RoleAction.Voiceban:
                    if (guildConfig.VoicebanRole.GetRole(context.Guild) != null && prompt)
                    {
                        previousRoleSet = true;
                        previousRoleType = RoleAction.Voiceban;
                        break;
                    }
                    else if (role == null)
                    {
                        role = await context.Guild.CreateRoleAsync("Voiceban", Permissions.None, DiscordColor.Gray, false, false, "Creating voiceban role...");
                    }
                    guildConfig.VoicebanRole = role.Id;
                    break;
                case RoleAction.None:
                default:
                    throw new NotImplementedException();
            }

            if (previousRoleSet && previousRoleType != RoleAction.None)
            {
                ulong previousRole = previousRoleType switch
                {
                    RoleAction.Antimeme => guildConfig.AntimemeRole,
                    RoleAction.Mute => guildConfig.MuteRole,
                    RoleAction.Voiceban => guildConfig.VoicebanRole,
                    RoleAction.None or _ => throw new NotImplementedException()
                };

                if (previousRole == role.Id)
                {
                    await Program.SendMessage(context, $"{role.Mention} is already the {previousRoleType} role!");
                    await FixPermissions(context.Guild, roleAction, role);
                }
                else
                {
                    DiscordMessage message = await Program.SendMessage(context, $"Role <@{previousRole}> is set as the current {previousRoleType} role. Should we change it?");
                    Queue queue = new(message, context.User, new(async eventArgs =>
                    {
                        if (eventArgs.TimedOut || eventArgs.MessageReactionAddEventArgs.Emoji == Constants.ThumbsDown)
                        {
                            await message.ModifyAsync(Formatter.Strike(message.Content) + '\n' + Formatter.Bold("[Notice: Aborting!]"));
                        }
                        else
                        {
                            await SetRole(context, roleAction, role, false);
                        }
                    }));
                }
                return;
            }

            Checklist checklist = new(context, "Saving role id to database...", "Override channel permissions for role...");
            await ModLog(context.Guild, LogType.Config, Database, $"{roleAction} Role => {context.User.Mention} has changed the {roleAction} role to {role.Mention}");
            await Database.SaveChangesAsync();
            await checklist.Check();
            await FixPermissions(context.Guild, roleAction, role);
            await checklist.Finalize($"Role {role.Mention} is now set as the {roleAction} role!");
            checklist.Dispose();
        }

        /// <summary>
        /// Fixes a <see cref="DiscordRole"/>'s <see cref="Permissions"/> for the entire <paramref name="discordGuild"/> to ensure that the <paramref name="discordRole"/> works as intended.
        /// </summary>
        /// <param name="discordGuild">The <see cref="DiscordGuild"/> in question.</param>
        /// <param name="roleAction">The <see cref="RoleAction"/> determines which set of <see cref="Permissions"/> to use.</param>
        /// <param name="discordRole">The <see cref="DiscordRole"/> whose <see cref="Permissions"/> should be fixed.</param>
        public static async Task FixPermissions(DiscordGuild discordGuild, RoleAction roleAction, DiscordRole discordRole)
        {
            foreach (DiscordChannel channel in discordGuild.Channels.Values)
            {
                await FixPermissions(channel, roleAction, discordRole);
            }
        }

        /// <summary>
        /// Fixes a <see cref="DiscordRole"/>'s <see cref="Permissions"/> for a specific <paramref name="discordChannel"/> to ensure that the <paramref name="discordRole"/> works as intended.
        /// </summary>
        /// <param name="discordChannel">The <see cref="DiscordChannel"/> in question.</param>
        /// <param name="roleAction">The <see cref="RoleAction"/> determines which set of <see cref="Permissions"/> to use.</param>
        /// <param name="discordRole">The <see cref="DiscordRole"/> whose <see cref="Permissions"/> should be fixed.</param>
        public static async Task FixPermissions(DiscordChannel discordChannel, RoleAction roleAction, DiscordRole discordRole)
        {
            string auditLogReason = roleAction switch
            {
                RoleAction.Mute => "Configuring permissions for mute role. Preventing role from sending messages, reacting to messages and speaking in voice channels.",
                RoleAction.Antimeme => "Configuring permissions for antimeme role. Preventing role from reacting to messages, embedding links and uploading files. In voice channels, preventing role from streaming and forcing push-to-talk.",
                RoleAction.Voiceban => "Configuring permissions for voiceban role. Preventing role from connecting to voice channels.",
                _ => "Not configuring unknown role action."
            };

            switch (roleAction)
            {
                case RoleAction.Antimeme:
                    if (discordChannel.Type == ChannelType.Category)
                    {
                        await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection, auditLogReason);
                    }
                    else if (discordChannel.Type == ChannelType.Voice)
                    {
                        await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.Stream | Permissions.UseVoiceDetection, auditLogReason);
                    }
                    else
                    {
                        await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis, auditLogReason);
                    }
                    break;
                case RoleAction.Mute:
                    if (discordChannel.Type == ChannelType.Category)
                    {
                        await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream, auditLogReason);
                    }
                    else if (discordChannel.Type == ChannelType.Voice)
                    {
                        await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.Speak | Permissions.Stream, auditLogReason);
                    }
                    else
                    {
                        await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions, auditLogReason);
                    }
                    break;
                case RoleAction.Voiceban:
                    if (discordChannel.Type is ChannelType.Category or ChannelType.Voice)
                    {
                        await discordChannel.AddOverwriteAsync(discordRole, Permissions.None, Permissions.UseVoice, auditLogReason);
                    }
                    else
                    {
                        DiscordOverwrite discordOverwrite = discordChannel.PermissionOverwrites.FirstOrDefault(overwrite => overwrite.Id == discordRole.Id);
                        if (discordOverwrite != null)
                        {
                            await discordOverwrite.DeleteAsync(auditLogReason);
                        }
                    }
                    break;
                case RoleAction.None:
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
