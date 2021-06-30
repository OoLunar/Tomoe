namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Threading.Tasks;

    public partial class Moderation : SlashCommandModule
    {
        public partial class Config : SlashCommandModule
        {
            public static async Task FixRolePermissions(DiscordGuild guild, DiscordRole role, CustomEvent roleType)
            {
                Permissions categoryPermissions;
                string auditLogReason;
                //Permissions voicePermissions = Permissions.None;
                //Permissions textPermissions = Permissions.None;

                switch (roleType)
                {
                    case CustomEvent.Mute:
                        categoryPermissions = Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream;
                        auditLogReason = "Configuring permissions for mute role. Preventing role from sending messages, reacting to messages and speaking in voice channels.";
                        //voicePermissions = Permissions.Speak | Permissions.Stream;
                        //textPermissions = Permissions.SendMessages | Permissions.AddReactions;
                        break;
                    case CustomEvent.Antimeme:
                        categoryPermissions = Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection;
                        auditLogReason = "Configuring permissions for antimeme role. Preventing role from reacting to messages, embedding links and uploading files. In voice channels, preventing role from streaming and forcing push-to-talk.";
                        //voicePermissions = Permissions.Stream | Permissions.UseVoiceDetection;
                        //textPermissions = Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis;
                        break;
                    case CustomEvent.Voiceban:
                        categoryPermissions = Permissions.UseVoice;
                        auditLogReason = "Configuring permissions for voiceban role. Preventing role from connecting to voice channels.";
                        //voicePermissions = Permissions.Speak | Permissions.Stream;
                        //textPermissions = Permissions.SendMessages | Permissions.AddReactions;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                foreach (DiscordChannel channel in guild.Channels.Values)
                {
                    await channel.AddOverwriteAsync(role, Permissions.None, categoryPermissions, auditLogReason);
                }
            }
        }
    }
}