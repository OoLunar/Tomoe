namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation.Attributes;
    using Tomoe.Db;
    using static Tomoe.Api.Moderation;

    public class UnMuteMemeBan : BaseCommandModule
    {
        public Database Database { private get; set; }

        [Command("unantimeme"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Description("Removes the `Antimeme` role from the victim. Note: Antimeme prevents the victim from reacting to messages, embedding links, uploading files, streaming and forces push-to-talk. The intention of this role is to prevent abuse of Discord's rich messaging features, or when someone is being really annoying by conversating with every known method except through words."), Punishment(false)]
        public async Task Unantimeme(CommandContext context, [Description("Who to remove the antimeme from.")] DiscordUser victim, [Description("Why is the victim's antimeme being taken away."), RemainingText] string antimemeReason = Constants.MissingReason) => await Punish(context, victim, antimemeReason, RoleAction.Antimeme);

        [Command("unmute"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("unsilence", "unshut", "unzip", "speak"), Description("Removes the `Muted` role from the victim. Note: A mute prevents the victim from sending messages, reacting to messages and speaking in voice channels."), Punishment(false)]
        public async Task Unmute(CommandContext context, [Description("Who to remove the mute from.")] DiscordUser victim, [Description("Why is the victim's mute being taken away."), RemainingText] string muteReason = Constants.MissingReason) => await Punish(context, victim, muteReason, RoleAction.Mute);

        [Command("unvoiceban"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.MoveMembers), Aliases("unvb"), Description("Removes the `Voicebanned` role, which prevents the victim from connecting to voice channels."), Punishment(false)]
        public async Task Unvoiceban(CommandContext context, [Description("Who to remove the voiceban from.")] DiscordUser victim, [Description("Why is the victim's voiceban being taken away."), RemainingText] string voicebanReason = Constants.MissingReason) => await Punish(context, victim, voicebanReason, RoleAction.Voiceban);

        public async Task Punish(CommandContext context, DiscordUser victim, string punishReason, RoleAction roleAction)
        {
            bool sentDm;
            try
            {
                sentDm = await UnmuteMemeBan(context.Guild, victim, context.User.Id, punishReason, roleAction, context.Message.JumpLink.ToString());
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

            await Program.SendMessage(context, $"{victim.Mention}'s {roleAction.ToString().ToLowerInvariant()} has been removed{(sentDm ? '.' : " (Failed to dm).")} Reason: ```\n{punishReason}```");
        }
    }
}