using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation {
    public class Ban : BaseCommandModule {
        [Command("ban"), Description("Bans a member from the guild.")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireGuild]
        public async Task BanUser(CommandContext context, [Description("The person to be banned.")] DiscordUser victim, [Description("(Optional) Removed the victim's messages from the pass `x` days.")] int pruneDays, [Description("(Optional) Why is the victim being banned.")][RemainingText] string banReason = Program.MissingReason) {
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
                context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason ?? Program.MissingReason);
            } catch (NotFoundException) {
                Program.SendMessage(context, $"Failed to message {victim.Mention} because they aren't in the guild anymore, but they have been banned. Reason:\n```{banReason.Filter(context, ExtensionMethods.FilteringAction.FilterAll) ?? Program.MissingReason}```", (ExtensionMethods.FilteringAction.RoleMentions | ExtensionMethods.FilteringAction.CodeBlocksIgnore));
                return;
            }
            Program.SendMessage(context, $"{victim.Mention} has been permanently banned. Reason:\n```{banReason.Filter(context, ExtensionMethods.FilteringAction.FilterAll) ?? Program.MissingReason}```", (ExtensionMethods.FilteringAction.RoleMentions | ExtensionMethods.FilteringAction.CodeBlocksIgnore));
        }

        [Command("ban")]
        [RequireGuild]
        public async Task BanUser(CommandContext context, [Description("The person to be banned.")] DiscordUser victim, [Description("(Optional) Why is the victim being banned."), RemainingText] string banReason = Program.MissingReason) => BanUser(context, victim, 0, banReason);

        [Command("ban")]
        [RequireGuild]
        public async Task BanUser(CommandContext context, [Description("The person to be banned.")] DiscordUser victim) => BanUser(context, victim, 0, null);

        [Command("ban")]
        [RequireGuild]
        public async Task BanUsers(CommandContext context, [Description("(Optional) Removed the victim's messages from the pass `x` days.")] int pruneDays = 7, [Description("(Optional) Why the people are getting banned.")] string banReason = Program.MissingReason, [Description("The people to be banned.")] params DiscordUser[] victims) {
            foreach (DiscordUser victim in victims) BanUser(context, victim, pruneDays, banReason);
            Program.SendMessage(context, $"Successfully massbanned {string.Join<DiscordUser>(", ", victims)}", ExtensionMethods.FilteringAction.RoleMentions);
        }
    }
}