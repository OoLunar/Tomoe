namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation.Attributes;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.Config;
    using static Tomoe.Commands.Moderation.ModLogs;

    public class UnMuteMemeBan : BaseCommandModule
    {
        public Database Database { private get; set; }

        [Command("unantimeme"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Description("Removes the `Antimeme` role from the victim. Note: Antimeme prevents the victim from reacting to messages, embedding links, uploading files, streaming and forces push-to-talk. The intention of this role is to prevent abuse of Discord's rich messaging features, or when someone is being really annoying by conversating with every known method except through words."), Punishment(false)]
        public async Task Unantimeme(CommandContext context, [Description("Who to remove the antimeme from.")] DiscordUser victim, [Description("Why is the victim's antimeme being taken away."), RemainingText] string antimemeReason = Constants.MissingReason) => await Punish(context, victim, antimemeReason, RoleAction.Antimeme, (await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id) ?? new GuildConfig(context.Guild.Id)).AntimemeRole.GetRole(context.Guild), $"You're antimeme has been removed from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(antimemeReason))}Context: {context.Message.JumpLink}\nNote: Antimeme prevents you from reacting to messages, sending embeds, uploading files, streaming to voice channels, and adds the push-to-talk restriction to voice channels.");

        [Command("unmute"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("unsilence", "unshut", "unzip", "speak"), Description("Removes the `Muted` role from the victim. Note: A mute prevents the victim from sending messages, reacting to messages and speaking in voice channels."), Punishment(false)]
        public async Task Unmute(CommandContext context, [Description("Who to remove the mute from.")] DiscordUser victim, [Description("Why is the victim's mute being taken away."), RemainingText] string muteReason = Constants.MissingReason) => await Punish(context, victim, muteReason, RoleAction.Mute, (await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id) ?? new GuildConfig(context.Guild.Id)).MuteRole.GetRole(context.Guild), $"You're mute has been removed from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}Context: {context.Message.JumpLink}\nNote: A mute prevents you from having any sort of interaction with the guild. It makes the entire guild readonly.");

        [Command("unvoiceban"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.MoveMembers), Aliases("unvb"), Description("Removes the `Voicebanned` role, which prevents the victim from connecting to voice channels."), Punishment(false)]
        public async Task Unvoiceban(CommandContext context, [Description("Who to remove the voiceban from.")] DiscordUser victim, [Description("Why is the victim's voiceban being taken away."), RemainingText] string voicebanReason = Constants.MissingReason) => await Punish(context, victim, voicebanReason, RoleAction.Voiceban, (await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id) ?? new GuildConfig(context.Guild.Id)).VoicebanRole.GetRole(context.Guild), $"You're voiceban has been removed from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(voicebanReason))}Context: {context.Message.JumpLink}\nNote: A voiceban prevents you from connecting to voice channels.");

        public async Task Punish(CommandContext context, DiscordUser victim, string punishReason, RoleAction roleAction, DiscordRole discordRole, string dmMessage)
        {
            if (discordRole == null)
            {
                await Program.SendMessage(context, Constants.MissingRole);
                return;
            }

            string rolename = roleAction switch
            {
                RoleAction.Mute => "unmuted",
                RoleAction.Antimeme => "unantimemed",
                RoleAction.Voiceban => "unvoicebanned",
                _ => "unpunished"
            };

            bool sentDm;
            try
            {
                sentDm = await ByProgram(context.Guild, victim, context.User, punishReason, roleAction, discordRole, dmMessage);
            }
            catch (ArgumentException error)
            {
                await Program.SendMessage(context, error.Message);
                return;
            }
            await Program.SendMessage(context, $"{victim.Mention} has been {rolename}{(sentDm ? '.' : " (Failed to dm).")} Reason: ```\n{punishReason}```");
        }

        public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, DiscordUser issuer, string punishReason, RoleAction roleAction, DiscordRole discordRole, string dmMessage)
        {
            DiscordMember discordVictim = await victim.Id.GetMember(discordGuild);
            bool sentDm = false;
            if (discordVictim != null)
            {
                await discordVictim.RevokeRoleAsync(discordRole, punishReason);
                sentDm = await discordVictim.TryDmMember(dmMessage);
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == discordGuild.Id) ?? new GuildConfig(discordGuild.Id);
            GuildUser databaseVictim = await database.GuildUsers.FirstOrDefaultAsync(guildUser => guildUser.UserId == victim.Id && guildUser.GuildId == discordGuild.Id) ?? new GuildUser(victim.Id);

            switch (roleAction)
            {
                case RoleAction.Antimeme:
                    databaseVictim.IsAntimemed = !databaseVictim.IsAntimemed ? throw new ArgumentException(Formatter.Bold("[Error]: User is not antimemed.")) : false;
                    break;
                case RoleAction.Mute:
                    databaseVictim.IsMuted = !databaseVictim.IsMuted ? throw new ArgumentException(Formatter.Bold("[Error]: User is not muted.")) : false;
                    break;
                case RoleAction.Voiceban:
                    databaseVictim.IsVoicebanned = !databaseVictim.IsVoicebanned ? throw new ArgumentException(Formatter.Bold("[Error]: User is not voicebanned.")) : false;
                    break;
                default:
                    throw new ArgumentException("Unable to determine which punishment to set.");
            }

            string rolename = roleAction switch
            {
                RoleAction.Mute => "unmuted",
                RoleAction.Antimeme => "unantimemed",
                RoleAction.Voiceban => "unvoicebanned",
                _ => "unpunished"
            };

            LogType logType = roleAction switch
            {
                RoleAction.Mute => LogType.Unmute,
                RoleAction.Antimeme => LogType.Unantimeme,
                RoleAction.Voiceban => LogType.Unvoiceban,
                _ => LogType.Unknown
            };

            await Record(discordGuild, logType, database, $"{issuer.Mention} has {rolename} {victim.Mention}{(sentDm ? '.' : " (Failed to dm).")} Reason: {punishReason}");
            await database.SaveChangesAsync();
            return sentDm;
        }
    }
}
