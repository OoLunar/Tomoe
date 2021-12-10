namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation : ApplicationCommandModule
    {
        public partial class Config : ApplicationCommandModule
        {
            public static async Task FixRolePermissions(DiscordGuild guild, DiscordMember discordMember, DiscordRole role, CustomEvent roleType, Database database)
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

                Dictionary<string, string> keyValuePairs = new();
                keyValuePairs.Add("guild_name", guild.Name);
                keyValuePairs.Add("guild_count", Public.TotalMemberCount[guild.Id].ToMetric());
                keyValuePairs.Add("guild_id", guild.Id.ToString(CultureInfo.InvariantCulture));
                keyValuePairs.Add("moderator_username", discordMember.Username);
                keyValuePairs.Add("moderator_tag", discordMember.Discriminator);
                keyValuePairs.Add("moderator_mention", discordMember.Mention);
                keyValuePairs.Add("moderator_id", discordMember.Id.ToString(CultureInfo.InvariantCulture));
                keyValuePairs.Add("moderator_displayname", discordMember.DisplayName);
                keyValuePairs.Add("role_mention", role.Mention);
                keyValuePairs.Add("role_name", role.Name);
                keyValuePairs.Add("role_id", role.Id.ToString(CultureInfo.InvariantCulture));
                keyValuePairs.Add("role_type", roleType.Humanize());
                await ModLog(guild, keyValuePairs, CustomEvent.RoleCreation, database, false);
            }
        }
    }
}