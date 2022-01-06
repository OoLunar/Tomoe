using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    public class MuteMemeBan : BaseCommandModule
    {
        public Database Database { private get; set; }

        [Command("antimeme"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Description("Grants the victim the `Antimeme` role, which prevents them from reacting to messages, embedding links, uploading files, streaming and forces push-to-talk. The intention of this role is to prevent abuse of Discord's rich messaging features, or when someone is being really annoying by conversating with every known method except through words."), Punishment(false)]
        public async Task Antimeme(CommandContext context, [Description("Who's being antimemed.")] DiscordUser victim, [Description("Why is the victim being antimeme'd."), RemainingText] string antimemeReason = Constants.MissingReason) => await Punish(context, victim, antimemeReason, Api.Moderation.RoleAction.Antimeme);

        [Command("mute"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("silence", "shut", "zip"), Description("Grants the victim the `Muted` role, which prevents them from sending messages, reacting to messages and speaking in voice channels."), Punishment(false)]
        public async Task Mute(CommandContext context, [Description("Who's being muted.")] DiscordUser victim, [Description("Why is the victim being muted."), RemainingText] string muteReason = Constants.MissingReason) => await Punish(context, victim, muteReason, Api.Moderation.RoleAction.Mute);

        [Command("voiceban"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.MoveMembers), Aliases("vb"), Description("Grants the victim the `Voicebanned` role, which prevents them from connecting to voice channels."), Punishment(false)]
        public async Task Voiceban(CommandContext context, [Description("Who's being voicebanned.")] DiscordUser victim, [Description("Why is the victim being voicebanned."), RemainingText] string voicebanReason = Constants.MissingReason) => await Punish(context, victim, voicebanReason, Api.Moderation.RoleAction.Voiceban);

        public async Task Punish(CommandContext context, DiscordUser victim, string punishReason, Api.Moderation.RoleAction roleAction)
        {
            if (await new Punishment(false).ExecuteCheckAsync(context, false))
            {
                bool sentDm;
                try
                {
                    sentDm = await Api.Moderation.MuteMemeBan(context.Guild, victim, context.User.Id, punishReason, roleAction, context.Message.JumpLink.ToString());
                }
                catch (ArgumentException error)
                {
                    await Program.SendMessage(context, error.Message);
                    return;
                }
                catch (MissingFieldException error)
                {
                    await Program.SendMessage(context, error.Message);
                    return;
                }

                await Program.SendMessage(context, $"{victim.Mention} has been given a {roleAction.ToString().ToLowerInvariant()}{(sentDm ? '.' : " (Failed to dm).")} Reason: ```\n{punishReason}```");
            }
        }
    }
}