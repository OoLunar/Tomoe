using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation {
    public class Unban : BaseCommandModule {
        [Command("unban"), Description("Unbans a user from the guild.")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [RequireBotPermissions(Permissions.BanMembers)]
        public async Task UnbanUser(CommandContext context, [Description("The member in question.")] DiscordUser victim, [Description("Why is the user being unbanned?"), RemainingText] string unbanReason) {
            try {
                context.Guild.UnbanMemberAsync(victim, unbanReason ?? Program.MissingReason);
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                (await guildVictim.CreateDmChannelAsync()).SendMessageAsync($"You've been unbanned by **{context.User.Mention}** from **{context.Guild.Name}**. Reason:\n```{unbanReason.Filter(context) ?? Program.MissingReason}```");
            } catch (NotFoundException) {
                Program.SendMessage(context, $"Failed to message {victim.Mention} because they aren't in the guild anymore, but they have been unbanned. Reason:\n```{unbanReason.Filter(context) ?? Program.MissingReason}```");
                return;
            }
            Program.SendMessage(context, $"{victim.Mention} has been unbanned. Reason:\n```{unbanReason.Filter(context) ?? Program.MissingReason}```");
        }

        [Command("unban")]
        public async Task UnbanUser(CommandContext context, [Description("The member in question.")] DiscordUser victim) => UnbanUser(context, victim, null);
    }
}