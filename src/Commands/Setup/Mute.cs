using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;
using Tomoe.Utils.Cache;
using Tomoe.Utils.Dialog;

namespace Tomoe.Commands.Setup {
    public class Mute : InteractiveBase {
        private GuildPermissions muteRolePerms = new GuildPermissions(addReactions: false, sendMessages: false, sendTTSMessages: false, embedLinks: false, attachFiles: false, mentionEveryone: false, useExternalEmojis: false);
        private OverwritePermissions muteChannelPerms = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny);

        /// <summary>
        /// Setup the mute role by pinging said role.
        /// <para>
        /// <code>
        /// >>setup_mute &lt;@&amp;role_id&gt;
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupMuteByRole(IRole newRole) {
            MutedRole currentRole = MutedRole.Get(Context.Guild.Id);
            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Issuer = Context.Guild.GetUser(Context.User.Id);
            dialogContext.OldRole = Context.Guild.GetRole(currentRole.RoleID);
            dialogContext.NewRole = newRole;

            if (newRole.Id == Context.Guild.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.NoEveryoneRole;
                await dialogContext.SendChannel();
                return;
            }

            // If no mute role is set
            if (currentRole == null) {
                MutedRole.Set(Context.Guild.Id, newRole.Id, Context.User.Id);
                foreach (IGuildChannel channel in Context.Guild.Channels)
                    if (channel is ITextChannel) await channel.AddPermissionOverwriteAsync(newRole, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                await dialogContext.SendChannel();
                return;
            }
            // If they tried replacing the old role with the same role.
            else if (currentRole.RoleID == newRole.Id) {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.FixPermissions;
                IUserMessage fixPermissions = await dialogContext.SendChannel();
                ReactionCallBack callBack = new ReactionCallBack();
                callBack.DialogContext = dialogContext;
                callBack.TakeAction = ReactionCallBack.ActionTypes.Boolean;
                callBack.Context = Context;
                // Add callback before reactions for hasty users
                Interactive.AddReactionCallback(fixPermissions, callBack);
                Emoji[] reactions = { new Emoji("☑"), new Emoji("❌") };
                await fixPermissions.AddReactionsAsync(reactions);
            } else {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.AlreadySetup;
                IUserMessage sendMessage = await dialogContext.SendChannel();
                ReactionCallBack roleCallback = new ReactionCallBack();
                roleCallback.DialogContext = dialogContext;
                Interactive.AddReactionCallback(sendMessage, roleCallback);
                Emoji[] reactions = { new Emoji(Program.Dialogs.Emotes.Yes), new Emoji(Program.Dialogs.Emotes.No) };
                await sendMessage.AddReactionsAsync(reactions);
            }
        }

        /// <summary>
        /// Setup the mute role by using said role's ID.
        /// <para>
        /// <code>
        /// >>setup_mute role_id
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetupMuteByID(ulong role) {
            MutedRole currentRole = MutedRole.Get(Context.Guild.Id);
            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Issuer = Context.Guild.GetUser(Context.User.Id);
            dialogContext.OldRole = Context.Guild.GetRole(currentRole.RoleID);

            IRole newRole = Context.Guild.GetRole(role);
            if (role.ToString() != null) await SetupMuteByRole(Context.Guild.GetRole(role));
            else {
                dialogContext.Error = Program.Dialogs.Message.Errors.NonExistingRole;
                await dialogContext.SendChannel();
            }
        }

        /// <summary>
        /// Get's the current mute role. Optionally, it will prompt to create a mute role if there is not one already, or it was deleted.
        /// <para>
        /// <code>
        /// >>setup_mute
        /// </code>
        /// </para>
        /// </summary>
        [Command("setup_mute", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CheckMuteRole() {
            MutedRole? muteRole = Tomoe.Utils.Cache.MutedRole.Get(Context.Guild.Id);

            Context dialogContext = new Context();
            dialogContext.Guild = Context.Guild;
            dialogContext.Channel = Context.Channel;
            dialogContext.Issuer = Context.Guild.GetUser(Context.User.Id);
            dialogContext.UserAction = Tomoe.Utils.Dialog.Context.Action.SetupMute;
            dialogContext.RequiredGuildPermission = GuildPermission.Administrator;
            if (muteRole.HasValue()) {
                dialogContext.OldRole = Context.Guild.GetRole(muteRole.RoleID);
                // Mute role exists in the database, but not on Discord. Likely the role was deleted.
                if (dialogContext.OldRole == null) {
                    dialogContext.Error = Program.Dialogs.Message.Setup.Errors.MissingRole;
                    IUserMessage callbackMessage = await dialogContext.SendChannel();
                    ReactionCallBack callbackReaction = new ReactionCallBack();
                    callbackReaction.Context = Context;
                    callbackReaction.DialogContext = dialogContext;
                    callbackReaction.TakeAction = ReactionCallBack.ActionTypes.Boolean;
                    callbackReaction.OnReaction += async delegate(object source, Context context, SocketReaction reaction) {
                        IRole muteRole = await context.Guild.CreateRoleAsync("Muted", muteRolePerms, null, false, false);
                        foreach (SocketTextChannel channel in (await context.Guild.GetTextChannelsAsync(CacheMode.AllowDownload))) channel.AddPermissionOverwriteAsync(muteRole, muteChannelPerms);
                        Tomoe.Utils.Cache.MutedRole.Set(context.Guild.Id, muteRole.Id, context.Issuer.Id);
                        context.NewRole = muteRole;
                        context.Error = Program.Dialogs.Message.Setup.Mute.SuccessSetup;
                        context.SendChannel();
                    };
                    // Add the callback before reactions (due to DAPI being weird with reaction timing)
                    Interactive.AddReactionCallback(callbackMessage, callbackReaction);
                    Emoji[] reactions = { new Emoji(Program.Dialogs.Emotes.Yes), new Emoji(Program.Dialogs.Emotes.No) };
                    callbackMessage.AddReactionsAsync(reactions);
                }
                // Role exists on both the database and Discord.
                else {
                    dialogContext.Error = Program.Dialogs.Message.Setup.Mute.AlreadySetup;
                    dialogContext.SendChannel();
                }
            }
            // Mute role was never made.
            else {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.NotSetup;
                IUserMessage callbackMessage = await dialogContext.SendChannel();
                ReactionCallBack callbackReaction = new ReactionCallBack();
                callbackReaction.Context = Context;
                callbackReaction.DialogContext = dialogContext;
                callbackReaction.TakeAction = ReactionCallBack.ActionTypes.Boolean;
                callbackReaction.OnReaction += async delegate(object source, Context context, SocketReaction reaction) {
                    IRole muteRole = await context.Guild.CreateRoleAsync("Muted", muteRolePerms, null, false, false);
                    foreach (SocketTextChannel channel in (await context.Guild.GetTextChannelsAsync(CacheMode.AllowDownload))) channel.AddPermissionOverwriteAsync(muteRole, muteChannelPerms);
                    Tomoe.Utils.Cache.MutedRole.Set(context.Guild.Id, muteRole.Id, context.Issuer.Id);
                    context.NewRole = muteRole;
                    context.Error = Program.Dialogs.Message.Setup.Mute.SuccessSetup;
                    context.SendChannel();
                };
                // Add the callback before reactions (due to DAPI being weird with reaction timing)
                Interactive.AddReactionCallback(callbackMessage, callbackReaction);
                Emoji[] reactions = { new Emoji(Program.Dialogs.Emotes.Yes), new Emoji(Program.Dialogs.Emotes.No) };
                callbackMessage.AddReactionsAsync(reactions);
            }
        }
    }
}