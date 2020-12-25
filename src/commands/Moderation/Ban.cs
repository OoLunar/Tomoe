using System.Collections.Generic;
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

        [Command(_COMMAND_NAME), Description(_COMMAND_DESC), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers)]
        public async Task BanUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim, [Description(_PURGED_DESC)] int pruneDays, [Description(_SINGLE_BAN_REASON), RemainingText] string banReason = Program.MissingReason) {
            if (victim == context.Client.CurrentUser) {
                Program.SendMessage(context, Program.SelfAction);
                return;
            }

            if (pruneDays < 7) pruneDays = 7;
            bool sentDm = true;
            try {
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                if (guildVictim.Hierarchy > context.Guild.CurrentMember.Hierarchy) {
                    Program.SendMessage(context, Program.Hierarchy);
                    return;
                } else if (!guildVictim.IsBot) await guildVictim.SendMessageAsync($"You've been banned by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{banReason.Filter() ?? Program.MissingReason}\n```");
            } catch (NotFoundException) {
                sentDm = false;
            } catch (BadRequestException) {
                sentDm = false;
            } catch (UnauthorizedException) {
                sentDm = false;
            }
            await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason ?? Program.MissingReason);
            Program.SendMessage(context, $"{victim.Mention} has been permanently banned{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{banReason.Filter(ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace) ?? Program.MissingReason}```\n", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim, [Description(_SINGLE_BAN_REASON), RemainingText] string banReason) => BanUser(context, victim, default, banReason);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUser(CommandContext context, [Description(_SINGLE_VICTIM_DESC)] DiscordUser victim, [Description(_PURGED_DESC)] int pruneDays = 7) => BanUser(context, victim, pruneDays, Program.MissingReason);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUsers(CommandContext context, [Description(_PURGED_DESC)] int pruneDays = 7, [Description(_MASS_BAN_REASON)] string banReason = Program.MissingReason, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) {
            if (pruneDays < 7) pruneDays = 7;
            List<IMention> mentions = new List<IMention>();
            foreach (DiscordUser victim in victims) {
                if (victim == context.Client.CurrentUser) {
                    Program.SendMessage(context, Program.SelfAction);
                    return;
                }

                try {
                    DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                    if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy) {
                        Program.SendMessage(context, Program.Hierarchy);
                        return;
                    }

                    await guildVictim.SendMessageAsync($"You've been banned by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{banReason.Filter() ?? Program.MissingReason}\n```");
                } catch (NotFoundException) { } catch (BadRequestException) { } catch (UnauthorizedException) { }
                await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason ?? Program.MissingReason);
                mentions.Add(new UserMention(victim.Id));
            }
            Program.SendMessage(context, $"Successfully massbanned {string.Join<DiscordUser>(", ", victims)}. Reason: ```\n{banReason.Filter()}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, mentions);
        }

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUsers(CommandContext context, [Description(_MASS_BAN_REASON)] string banReason = Program.MissingReason, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) => BanUsers(context, default, banReason, victims);

        [Command(_COMMAND_NAME), RequireGuild]
        public async Task BanUsers(CommandContext context, [Description(_MASS_VICTIM_DESC)] params DiscordUser[] victims) => BanUsers(context, default, default, victims);
    }
}