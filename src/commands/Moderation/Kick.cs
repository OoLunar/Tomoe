using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation {
    public class Kick : BaseCommandModule {
        [Command("kick"), Description("Kicks a member from the guild.")]
        [RequireUserPermissions(Permissions.BanMembers)]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireGuild]
        [Priority(10)]
        public async Task KickUser(CommandContext context, [Description("The member in question.")] DiscordUser victim, [Description("(Optional) Why is the victim being kicked.")][RemainingText] string kickReason) {
            if (victim == context.Client.CurrentUser) {
                Program.SendMessage(context, Program.SelfAction);
                return;
            }
            try {
                DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
                (await guildVictim.CreateDmChannelAsync()).SendMessageAsync($"You've been kicked by **{context.User.Mention}** from **{context.Guild.Name}**. Reason:\n```{kickReason.Filter(context) ?? Program.MissingReason}```");
                guildVictim.RemoveAsync(kickReason ?? Program.MissingReason);
            } catch (NotFoundException) {
                Program.SendMessage(context, $"Failed to message {victim.Mention} because they aren't in the guild anymore, but they have been kicked. Reason:\n```{kickReason.Filter(context) ?? Program.MissingReason}```");
                return;
            }
            Program.SendMessage(context, $"{victim.Mention} has been kicked. Reason:\n```{kickReason.Filter(context) ?? Program.MissingReason}```");
        }

        [Command("kick")]
        [RequireBotPermissions(Permissions.BanMembers)]
        [RequireUserPermissions(Permissions.BanMembers)]
        [RequireGuild]
        [Priority(9)]
        public async Task KickUser(CommandContext context, [Description("The member in question.")] DiscordUser victim) => KickUser(context, victim, null);
    }
}