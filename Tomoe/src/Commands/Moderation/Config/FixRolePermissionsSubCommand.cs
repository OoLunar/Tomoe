using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    public partial class ConfigCommand : ApplicationCommandModule
    {
        public static async Task FixRolePermissionsAsync(DiscordGuild guild, DiscordMember discordMember, DiscordRole role, CustomEvent roleType, Database database)
        {
            Permissions categoryPermissions;
            string auditLogReason;

            switch (roleType)
            {
                case CustomEvent.Mute:
                    categoryPermissions = Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream;
                    auditLogReason = "Configuring permissions for mute role. Preventing role from sending messages, reacting to messages and speaking in voice channels.";
                    break;
                case CustomEvent.Antimeme:
                    categoryPermissions = Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection;
                    auditLogReason = "Configuring permissions for antimeme role. Preventing role from reacting to messages, embedding links and uploading files. In voice channels, preventing role from streaming and forcing push-to-talk.";
                    break;
                case CustomEvent.Voiceban:
                    categoryPermissions = Permissions.UseVoice;
                    auditLogReason = "Configuring permissions for voiceban role. Preventing role from connecting to voice channels.";
                    break;
                default:
                    throw new NotImplementedException();
            }

            foreach (DiscordChannel channel in guild.Channels.Values)
            {
                await channel.AddOverwriteAsync(role, Permissions.None, categoryPermissions, auditLogReason);
            }

            Dictionary<string, string> keyValuePairs = new()
                {
                    { "guild_name", guild.Name },
                    { "guild_count", Program.TotalMemberCount[guild.Id].ToMetric() },
                    { "guild_id", guild.Id.ToString(CultureInfo.InvariantCulture) },
                    { "moderator_username", discordMember.Username },
                    { "moderator_tag", discordMember.Discriminator },
                    { "moderator_mention", discordMember.Mention },
                    { "moderator_id", discordMember.Id.ToString(CultureInfo.InvariantCulture) },
                    { "moderator_displayname", discordMember.DisplayName },
                    { "role_mention", role.Mention },
                    { "role_name", role.Name },
                    { "role_id", role.Id.ToString(CultureInfo.InvariantCulture) },
                    { "role_type", roleType.Humanize() }
                };
            await ModLogCommand.ModLogAsync(guild, keyValuePairs, CustomEvent.RoleCreation, database, false);
        }
    }
}
