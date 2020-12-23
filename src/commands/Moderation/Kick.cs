using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation {
    public class Kick : BaseCommandModule {
        private const string _COMMAND_NAME = "kick";
        private const string _COMMAND_DESC = "Kicks people from the guild, sending them off with a private message.";
        private const string _SINGLE_VICTIM_DESC = "The person to be kicked.";
        private const string _SINGLE_KICK_REASON = "(Optional) The reason why the person is being kicked.";
        private const string _MASS_VICTIM_DESC = "The people to be kicked.";
        private const string _MASS_KICK_REASON = "(Optional) The reason why people are being kicked.";

        [Command(_COMMAND_NAME), Description(_COMMAND_DESC), RequireGuild]
        [RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers)]
        public async Task KickUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim, [Description(_SINGLE_KICK_REASON)][RemainingText] string kickReason) {
            if (victim == context.Client.CurrentUser) {
                Program.SendMessage(context, Program.SelfAction);
                return;
            }
            //TODO: Clean this up
            try {
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                try {
                    if (guildVictim.Hierarchy > context.Guild.CurrentMember.Hierarchy) {
                        Program.SendMessage(context, Program.Hierarchy);
                        return;
                    } else if (!guildVictim.IsBot) await guildVictim.SendMessageAsync($"You've been kicked by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{kickReason.Filter(context) ?? Program.MissingReason}\n```");
                } catch (UnauthorizedException) { }
                guildVictim.RemoveAsync(kickReason);
            } catch (NotFoundException) {
                Program.SendMessage(context, $"Failed to kick {victim.Mention} since they aren't in the guild. Kick Reason:\n```{kickReason.Filter(context) ?? Program.MissingReason}```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
                return;
            }
            Program.SendMessage(context, $"{victim.Mention} has been kicked. Reason: ```\n{kickReason.Filter(context) ?? Program.MissingReason}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task KickUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim) => KickUser(context, victim, null);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task KickUsers(CommandContext context, [Description(_MASS_KICK_REASON)] string kickReason = Program.MissingReason, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) {
            foreach (DiscordUser victim in victims) KickUser(context, victim, kickReason);
            Program.SendMessage(context, $"Successfully masskicked {string.Join<DiscordUser>(", ", victims)}. Reason: ```\n{kickReason.Filter(context)}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, victims.Select(user => new UserMention(user.Id) as IMention).ToList());
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task KickUsers(CommandContext context, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) => KickUsers(context, default, victims);
    }
}