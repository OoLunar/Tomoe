using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils.Dialog;

namespace Tomoe.Commands.Moderation {
    public class Strike : InteractiveBase {

        [Command("strike", RunMode = RunMode.Async)]
        [Summary("Strikes a member.")]
        [Remarks("Moderation")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByID(ulong victimId, [Remainder] string reason = null) {
            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.Strike;
            dialogContext.RequiredGuildPermission = GuildPermission.KickMembers;
            dialogContext.Reason = reason;

            SocketGuildUser muteMember = Context.Guild.GetUser(victimId);

            // Victim does not exist.
            if (dialogContext.Victim == null) return;
            // Victim is not in the guild.
            else if (muteMember == null) {
                dialogContext.Error = Program.Dialogs.Message.Errors.NotInGuild;
                dialogContext.SendChannel();
                return;
            }
            // Check for bot self mute.
            else if (dialogContext.Victim.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }
            // Check if bot can mute user.
            else if (muteMember != null && muteMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }

            try {
                Utils.Cache.Strikes.Add(Context.Guild.Id, dialogContext.Victim.Id);

                // Let the victim know they were unmuted.
                await dialogContext.SendDM();
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            dialogContext.SendChannel();
            return;
        }

        [Command("strike", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task ByMention(SocketGuildUser victim, [Remainder] string reason = null) => await ByID(victim.Id, reason);
    }
}