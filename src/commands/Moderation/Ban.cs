using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation {
    public class Ban : BaseCommandModule {
        private const string _COMMAND_NAME = "ban";
        private const string _COMMAND_DESC = "Bans people from the guild, sending them off with a private message.";
        private const string _PURGED_DESC = "(Optional) Removed the victim's messages from the pass `x` days.";
        private const string _SINGLE_VICTIM_DESC = "The person to be banned.";
        private const string _SINGLE_BAN_REASON = "(Optional) The reason why the person is being banned.";
        private const string _MASS_VICTIM_DESC = "The people to be banned.";
        private const string _MASS_BAN_REASON = "(Optional) The reason why the people are being banned.";

        [Command(_COMMAND_NAME), Description(_COMMAND_DESC), RequireGuild]
        [RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers)]
        public async Task BanUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim, [Description(_PURGED_DESC)] int pruneDays, [Description(_SINGLE_BAN_REASON), RemainingText] string banReason = Program.MissingReason) {
            if (victim == context.Client.CurrentUser) {
                Program.SendMessage(context, Program.SelfAction);
                return;
            }

            if (pruneDays < 7) pruneDays = 7;
            try {
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy) {
                    Program.SendMessage(context, Program.Hierarchy);
                    return;
                }
                (await guildVictim.CreateDmChannelAsync()).SendMessageAsync($"You've been banned by **{context.User.Mention}** from **{context.Guild.Name}**. Reason:\n```{banReason.Filter(context) ?? Program.MissingReason}```");
                await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason ?? Program.MissingReason);
            } catch (NotFoundException) {
                await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason ?? Program.MissingReason);
                Program.SendMessage(context, $"Failed to message {victim.Mention} because they aren't in the guild anymore, but they have been banned. Reason:\n```{banReason.Filter(context, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace) ?? Program.MissingReason}```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
                return;
            }
            Program.SendMessage(context, $"{victim.Mention} has been permanently banned. Reason:\n```{banReason.Filter(context, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace) ?? Program.MissingReason}```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new System.Collections.Generic.List<IMention>() { new UserMention(victim.Id) });
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim, [Description(_SINGLE_BAN_REASON), RemainingText] string banReason) => BanUser(context, victim, default, banReason);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim, [Description(_PURGED_DESC)] int pruneDays = 7) => BanUser(context, victim, pruneDays, Program.MissingReason);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUsers(CommandContext context, [Description(_PURGED_DESC)] int pruneDays = 7, [Description(_MASS_BAN_REASON)] string banReason = Program.MissingReason, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) {
            foreach (DiscordUser victim in victims) BanUser(context, victim, pruneDays, banReason);
            Program.SendMessage(context, $"Successfully massbanned {string.Join<DiscordUser>(", ", victims)}. Reason:\n```\n{banReason.Filter(context)}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, victims.Select(user => new UserMention(user.Id) as IMention).ToList());
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUsers(CommandContext context, [Description(_MASS_BAN_REASON)] string banReason = Program.MissingReason, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) => BanUsers(context, default, banReason, victims);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUsers(CommandContext context, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) => BanUsers(context, default, default, victims);
    }
}