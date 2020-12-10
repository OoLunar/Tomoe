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
            try {
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                (await guildVictim.CreateDmChannelAsync()).SendMessageAsync($"You've been kicked by **{context.User.Mention}** from **{context.Guild.Name}**. Reason:\n```{kickReason.Filter(context) ?? Program.MissingReason}```");
                guildVictim.RemoveAsync(kickReason ?? Program.MissingReason);
            } catch (NotFoundException) {
                Program.SendMessage(context, $"Failed to kick them because they aren't in the guild. Kick Reason:\n```{kickReason.Filter(context) ?? Program.MissingReason}```", (ExtensionMethods.FilteringAction.RoleMentions | ExtensionMethods.FilteringAction.CodeBlocksIgnore));
                return;
            }
            Program.SendMessage(context, $"{victim.Mention} has been kicked. Reason:\n```{kickReason.Filter(context) ?? Program.MissingReason}```", (ExtensionMethods.FilteringAction.RoleMentions | ExtensionMethods.FilteringAction.CodeBlocksIgnore));
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task KickUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim) => KickUser(context, victim, null);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task KickUsers(CommandContext context, [Description(_MASS_KICK_REASON)] string kickReason = Program.MissingReason, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) {
            foreach (DiscordUser victim in victims) KickUser(context, victim, kickReason);
            Program.SendMessage(context, $"Successfully masskicked {string.Join<DiscordUser>(", ", victims)}", ExtensionMethods.FilteringAction.RoleMentions);
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task KickUsers(CommandContext context, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) => KickUsers(context, default, victims);
    }
}