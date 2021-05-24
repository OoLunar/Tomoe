namespace Tomoe.Api
{
    using DSharpPlus;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public class Config
        {
            public enum ConfigSetting
            {
                AdminRoles,
                AllowedInvites,
                GuildPrefixes,
                IgnoredChannels,
                AntiInvite,
                AutoDehoist,
                AutoStrike,
                AutoDelete,
                MaxLines,
                MaxMentions,
                ShowPermissionErrors,
                AntimemeRole,
                MuteRole,
                VoicebanRole
            }

            public static object Get(ulong discordGuildId, ConfigSetting configSetting)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                GuildConfig guildConfig = database.GuildConfigs.AsNoTracking().FirstOrDefault(guildConfig => guildConfig.Id == discordGuildId);
                return configSetting switch
                {
                    ConfigSetting.AdminRoles => guildConfig.AdminRoles,
                    ConfigSetting.AllowedInvites => guildConfig.AllowedInvites,
                    ConfigSetting.GuildPrefixes => guildConfig.Prefixes,
                    ConfigSetting.IgnoredChannels => guildConfig.IgnoredChannels,
                    ConfigSetting.AntimemeRole => guildConfig.AntimemeRole,
                    ConfigSetting.MuteRole => guildConfig.MuteRole,
                    ConfigSetting.VoicebanRole => guildConfig.VoicebanRole,
                    ConfigSetting.MaxLines => guildConfig.MaxLinesPerMessage,
                    ConfigSetting.MaxMentions => guildConfig.MaxUniqueMentionsPerMessage,
                    ConfigSetting.AntiInvite => guildConfig.AntiInvite,
                    ConfigSetting.AutoDehoist => guildConfig.AutoDehoist,
                    ConfigSetting.AutoDelete => guildConfig.AutoDelete,
                    ConfigSetting.AutoStrike => guildConfig.AutoStrike,
                    ConfigSetting.ShowPermissionErrors => guildConfig.ShowPermissionErrors,
                    _ => throw new ArgumentException("Unknown ConfigSetting! Open up a GitHub issue please.")
                };
            }

            public static async Task Set(DiscordClient discordClient, ulong discordGuildId, ulong discordUserId, ConfigSetting configSetting, object value)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                GuildConfig guildConfig = database.GuildConfigs.FirstOrDefault(guildConfig => guildConfig.Id == discordGuildId);
                object _ = configSetting switch
                {
                    ConfigSetting.AdminRoles => guildConfig.AdminRoles = (List<ulong>)value,
                    ConfigSetting.AllowedInvites => guildConfig.AllowedInvites = (List<string>)value,
                    ConfigSetting.GuildPrefixes => guildConfig.Prefixes = (List<string>)value,
                    ConfigSetting.IgnoredChannels => guildConfig.IgnoredChannels = (List<ulong>)value,
                    ConfigSetting.AntimemeRole => guildConfig.AntimemeRole = (ulong)value,
                    ConfigSetting.MuteRole => guildConfig.MuteRole = (ulong)value,
                    ConfigSetting.VoicebanRole => guildConfig.VoicebanRole = (ulong)value,
                    ConfigSetting.MaxLines => guildConfig.MaxLinesPerMessage = (int)value,
                    ConfigSetting.MaxMentions => guildConfig.MaxUniqueMentionsPerMessage = (int)value,
                    ConfigSetting.AntiInvite => guildConfig.AntiInvite = (bool)value,
                    ConfigSetting.AutoDehoist => guildConfig.AutoDehoist = (bool)value,
                    ConfigSetting.AutoDelete => guildConfig.AutoDelete = (bool)value,
                    ConfigSetting.AutoStrike => guildConfig.AutoStrike = (bool)value,
                    ConfigSetting.ShowPermissionErrors => guildConfig.ShowPermissionErrors = (bool)value,
                    _ => throw new ArgumentException("Unknown ConfigSetting! Open up a GitHub issue please.")
                };
                database.Entry(guildConfig).State = EntityState.Modified;
                await ModLog(discordClient, discordGuildId, LogType.Config, database, $"<@{discordUserId}> set {configSetting.Humanize()} to {value}");
                await database.SaveChangesAsync();
            }

            public static async Task AddList(DiscordClient discordClient, ulong discordGuildId, ulong discordUserId, ConfigSetting configSetting, object value)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                GuildConfig guildConfig = database.GuildConfigs.FirstOrDefault(guildConfig => guildConfig.Id == discordGuildId);
                switch (configSetting)
                {
                    case ConfigSetting.AdminRoles:
                        guildConfig.AdminRoles.Add((ulong)value);
                        break;
                    case ConfigSetting.AllowedInvites:
                        guildConfig.AllowedInvites.Add((string)value);
                        break;
                    case ConfigSetting.GuildPrefixes:
                        guildConfig.Prefixes.Add((string)value);
                        break;
                    case ConfigSetting.IgnoredChannels:
                        guildConfig.IgnoredChannels.Add((ulong)value);
                        break;
                    default:
                        throw new ArgumentException("ConfigSetting expected to be a list.");
                }
                database.Entry(guildConfig).State = EntityState.Modified;
                await ModLog(discordClient, discordGuildId, LogType.Config, database, $"<@{discordUserId}> added {value} to the {configSetting.Humanize()} list");
                await database.SaveChangesAsync();
            }

            public static async Task<bool> RemoveList(DiscordClient discordClient, ulong discordGuildId, ulong discordUserId, ConfigSetting configSetting, object value)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                GuildConfig guildConfig = database.GuildConfigs.FirstOrDefault(guildConfig => guildConfig.Id == discordGuildId);
                bool removed = configSetting switch
                {
                    ConfigSetting.AdminRoles => guildConfig.AdminRoles.Remove((ulong)value),
                    ConfigSetting.AllowedInvites => guildConfig.AllowedInvites.Remove((string)value),
                    ConfigSetting.GuildPrefixes => guildConfig.Prefixes.Remove((string)value),
                    ConfigSetting.IgnoredChannels => guildConfig.IgnoredChannels.Remove((ulong)value),
                    _ => throw new ArgumentException("ConfigSetting expected to be a list.")
                };
                database.Entry(guildConfig).State = EntityState.Modified;
                await ModLog(discordClient, discordGuildId, LogType.Config, database, $"<@{discordUserId}> removed {value} to the {configSetting.Humanize()} list");
                await database.SaveChangesAsync();
                return removed;
            }

            public static async Task ClearList(DiscordClient discordClient, ulong discordGuildId, ulong discordUserId, ConfigSetting configSetting)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                GuildConfig guildConfig = database.GuildConfigs.FirstOrDefault(guildConfig => guildConfig.Id == discordGuildId);
                switch (configSetting)
                {
                    case ConfigSetting.AdminRoles:
                        guildConfig.AdminRoles.Clear();
                        break;
                    case ConfigSetting.AllowedInvites:
                        guildConfig.AllowedInvites.Clear();
                        break;
                    case ConfigSetting.GuildPrefixes:
                        guildConfig.Prefixes.Clear();
                        break;
                    case ConfigSetting.IgnoredChannels:
                        guildConfig.IgnoredChannels.Clear();
                        break;
                    default:
                        throw new ArgumentException("ConfigSetting expected to be a list.");
                }
                database.Entry(guildConfig).State = EntityState.Modified;
                await ModLog(discordClient, discordGuildId, LogType.Config, database, $"<@{discordUserId}> cleared the {configSetting.Humanize()} list");
                await database.SaveChangesAsync();
            }

            public static object GetDefault(ConfigSetting configSetting)
            {
                return configSetting switch
                {
                    ConfigSetting.AdminRoles => new List<ulong>(),
                    ConfigSetting.AllowedInvites => new List<string>(),
                    ConfigSetting.GuildPrefixes => new List<string>() { ">>" },
                    ConfigSetting.IgnoredChannels => new List<ulong>(),
                    ConfigSetting.AntimemeRole => 0,
                    ConfigSetting.MuteRole => 0,
                    ConfigSetting.VoicebanRole => 0,
                    ConfigSetting.MaxLines => 20,
                    ConfigSetting.MaxMentions => 5,
                    ConfigSetting.AntiInvite => true,
                    ConfigSetting.AutoDehoist => true,
                    ConfigSetting.AutoDelete => false,
                    ConfigSetting.AutoStrike => true,
                    ConfigSetting.ShowPermissionErrors => true,
                    _ => throw new ArgumentException("Unknown ConfigSetting! Open up a GitHub issue please.")
                };
            }
        }
    }
}