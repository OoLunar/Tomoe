using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation {
    public class Kick : InteractiveBase {

        /// <summary>
        /// Kicks a guild member identified by a mention, optionally with a reason.
        /// <para>
        /// Prerequisites: Having the correct permissions: `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) and `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick &lt;@336733686529654798&gt; shitposting
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        [Summary("[Kicks a user through a mention or id.](https://github.com/OoLunar/Tomoe/tree/master/docs/moderation/kick.md)")]
        [Remarks("Moderation")]
        public async Task ByID(ulong victimId, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = await Program.Client.Rest.GetUserAsync(victimId);
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.RequiredGuildPermission = GuildPermission.KickMembers;
            dialogContext.Reason = reason;

            SocketGuildUser kickMember = Context.Guild.GetUser(victimId);

            // Victim does not exist.
            if (dialogContext.Victim == null) return;
            // Victim is not in the guild.
            else if (kickMember == null) {
                dialogContext.Error = Program.Dialogs.Message.Errors.NotInGuild;
                dialogContext.SendChannel();
                return;
            }

            // Check for bot self kick.
            else if (dialogContext.Victim.Id == Program.Client.CurrentUser.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedSelfBot;
                await dialogContext.SendChannel();
                return;
            }
            // Check if bot can kick the victim.
            else if (kickMember != null && kickMember.Hierarchy >= Context.Guild.GetUser(Program.Client.CurrentUser.Id).Hierarchy) {
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedHierarchyError;
                await dialogContext.SendChannel();
                return;
            }

            try {
                // Let the victim know they were kickned.
                await dialogContext.SendDM();
                // Remove DM error. TODO: Fix this. Check SendDM method.
                dialogContext.Error = null;
                // Kick the user
                await kickMember.KickAsync(reason);
            } catch (Discord.Net.HttpException error) when(error.DiscordCode.HasValue && error.DiscordCode == 50007) {
                // If the victim cannot be DM'd, let the issuer know.
                dialogContext.Error = Program.Dialogs.Message.Errors.FailedToDm;
            }

            dialogContext.SendChannel();
            return;
        }

        [Command("kick", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ByMention(Mention victim, int pruneDays = 7, [Remainder] string reason = null) => await ByID(victim.Id, reason);

        /// <summary>
        /// Informs the issuer that a kick could not be issued due to lack of bot/user permissions.
        /// <para>
        /// Prerequisites: Lacking role permissions of either `<see cref="Discord.GuildPermission.KickMembers"/>` (bot) or `<see cref="Discord.GuildPermission.KickMembers"/>` (user).
        /// <code>
        /// >>kick
        /// </code>
        /// </para>
        /// </summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task KickNoPerms(SocketGuildUser victim, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = victim;
            dialogContext.Reason = reason;
            dialogContext.RequiredGuildPermission = GuildPermission.KickMembers;

            if (!Context.Guild.GetUser(Program.Client.CurrentUser.Id).GuildPermissions.KickMembers)
                dialogContext.Error = Program.Dialogs.Message.Errors.NoBotPerms;
            else if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.KickMembers)
                dialogContext.Error = Program.Dialogs.Message.Errors.NoUserPerms;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the user that a kick cannot be issued in DMs.</summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task DM(IUser victim, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = victim;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }

        /// <summary>Informs the issuer that a kick cannot be issued in Group Chats.</summary>
        [Command("kick", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Group)]
        public async Task Group(IUser victim, [Remainder] string reason = null) {
            DialogContext dialogContext = new DialogContext();
            dialogContext.Channel = Context.Channel;
            dialogContext.UserAction = DialogContext.Action.Kick;
            dialogContext.Issuer = Context.User;
            dialogContext.Victim = victim;
            dialogContext.Reason = reason;
            dialogContext.Error = Program.Dialogs.Message.Errors.FailedInDm;

            await dialogContext.SendChannel();
            return;
        }
    }
}