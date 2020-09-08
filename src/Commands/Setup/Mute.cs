using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Setup {
    public class Mute : InteractiveBase {

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
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Issuer = Context.Guild.GetUser(Context.User.Id);
            dialogContext.OldRole = Context.Guild.GetRole(currentRole.RoleID);
            dialogContext.NewRole = newRole;

            if (newRole.Id == Context.Guild.Id) {
                dialogContext.Error = Program.Dialogs.Message.Errors.NoEveryoneRole;
                await dialogContext.SendChannel();
                return;
            }

            //If no mute role is set
            if (currentRole == null) {
                MutedRole.Store(Context.Guild.Id, newRole.Id, Context.User.Id);
                foreach (IGuildChannel channel in Context.Guild.Channels)
                    if (channel is ITextChannel) await channel.AddPermissionOverwriteAsync(newRole, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                await dialogContext.SendChannel();
                return;
            }
            //If they tried replacing the old role with the same role.
            else if (currentRole.RoleID == newRole.Id) {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.FixPermissions;
                IUserMessage fixPermissions = await dialogContext.SendChannel();
                ReactionCallBack callBack = new ReactionCallBack();
                callBack.DialogContext = dialogContext;
                callBack.TakeAction = takeAction.FixPermissions;
                callBack.Context = Context;
                //Add callback before reactions for hasty users
                Interactive.AddReactionCallback(fixPermissions, callBack);
                Emoji[] reactions = { new Emoji("☑"), new Emoji("❌") };
                await fixPermissions.AddReactionsAsync(reactions);
            } else {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.AlreadySetup;
                IUserMessage sendMessage = await dialogContext.SendChannel();
                ReactionCallBack roleCallback = new ReactionCallBack();
                roleCallback.DialogContext = dialogContext;
                Interactive.AddReactionCallback(sendMessage, roleCallback);
                Emoji[] reactions = { new Emoji("☑"), new Emoji("❌") };
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
            DialogContext dialogContext = new DialogContext();
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
            MutedRole currentRole = MutedRole.Get(Context.Guild.Id);
            DialogContext dialogContext = new DialogContext();
            dialogContext.Guild = Context.Guild;
            dialogContext.Issuer = Context.Guild.GetUser(Context.User.Id);
            dialogContext.OldRole = Context.Guild.GetRole(currentRole.RoleID);

            //Role was never set
            if (currentRole == null) {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.NotSetup;
                IUserMessage createRole = await dialogContext.SendChannel();
                ReactionCallBack callBack = new ReactionCallBack();
                callBack.DialogContext = dialogContext;
                callBack.TakeAction = takeAction.CreateRole;
                callBack.Context = Context;
                //Add callback before reactions for hasty users
                Interactive.AddReactionCallback(createRole, callBack);
                Emoji[] reactions = { new Emoji("☑"), new Emoji("❌") };
                await createRole.AddReactionsAsync(reactions);
            }
            //Role can't be found
            else if (dialogContext.OldRole == null) {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.MissingRole;
                IUserMessage missingRole = await dialogContext.SendChannel();
                ReactionCallBack callback = new ReactionCallBack();
                callback.DialogContext = dialogContext;
                callback.TakeAction = takeAction.CreateRole;
                callback.Context = Context;
                //Add callback before reactions for hasty users
                Interactive.AddReactionCallback(missingRole, callback);
                Emoji[] reactions = { new Emoji("☑"), new Emoji("❌") };
                await missingRole.AddReactionsAsync(reactions);
            }
            //Role exists
            else {
                dialogContext.Error = Program.Dialogs.Message.Setup.Mute.AlreadySetup;
                await dialogContext.SendChannel();
            }
        }
    }

    enum takeAction {
        CreateRole,
        AssignRole,
        FixPermissions
    }

    class ReactionCallBack : IReactionCallback {
        public SocketCommandContext Context { get; set; }
        public RunMode RunMode => RunMode.Async;
        public ICriterion<SocketReaction> Criterion => new EmptyCriterion<SocketReaction>();
        public TimeSpan? Timeout => System.TimeSpan.FromSeconds(10);
        public DialogContext DialogContext;
        public takeAction TakeAction;

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
            //Check if it's the same person reacting. Returning false means keep listening for reactions.
            if (reaction.UserId != DialogContext.Issuer.Id) {
                //Remove outsiders reaction regardless of emote, as they aren't the one who initiated the command.
                await reaction.Channel.GetCachedMessage(reaction.MessageId).RemoveReactionAsync(reaction.Emote, reaction.UserId);
                return false;
            }
            //Remove any and all reactions that aren't ❌ or ☑
            else if (reaction.Emote.Name != "❌" && reaction.Emote.Name != "☑") {
                await (await reaction.Channel.GetMessageAsync(reaction.MessageId)).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                return false;
            }
            //Exit
            else if (reaction.Emote.Name == "❌") {
                DialogContext.Error = Program.Dialogs.Message.Setup.Mute.Exit;
                DialogContext.Channel = reaction.Channel;
                await DialogContext.SendChannel();
                return true;
            }

            SocketGuildChannel reactChannel = reaction.Channel as SocketGuildChannel;
            GuildPermissions muteRolePerms = new GuildPermissions(addReactions: false, sendMessages: false, sendTTSMessages: false, embedLinks: false, attachFiles: false, mentionEveryone: false, useExternalEmojis: false);
            OverwritePermissions muteChannelPerms = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny);

            //Remove old role's perms and assign new one
            if (TakeAction == takeAction.AssignRole) {
                IUserMessage roleCreatedMessage = await reaction.Channel.SendMessageAsync("Applying permissions...");
                foreach (IGuildChannel channel in reactChannel.Guild.Channels) {
                    if (channel is ITextChannel) {
                        await channel.RemovePermissionOverwriteAsync(reactChannel.Guild.GetRole(DialogContext.OldRole.Id));
                        await channel.AddPermissionOverwriteAsync(DialogContext.NewRole, muteChannelPerms);
                    }
                }
                //Cache new role
                MutedRole.Store(reactChannel.Guild.Id, DialogContext.NewRole.Id, reaction.UserId);
                await roleCreatedMessage.ModifyAsync(m => {
                    DialogContext.UserAction = DialogContext.Action.SetupMute;
                    m.Content = DialogContext.Filter();
                });
                return true;
            }
            //Create the role for the user.
            else if (TakeAction == takeAction.CreateRole) {
                IUserMessage roleCreatedMessage = await reaction.Channel.SendMessageAsync("Creating Role...");
                DialogContext.NewRole = await Context.Guild.CreateRoleAsync("Muted", muteRolePerms, Color.DarkGrey, false, false);
                await roleCreatedMessage.ModifyAsync(m => { m.Content = "Applying permissions..."; });
                foreach (IGuildChannel channel in reactChannel.Guild.Channels)
                    if (channel is ITextChannel) await channel.AddPermissionOverwriteAsync(DialogContext.NewRole, muteChannelPerms);
                //Cache new role
                MutedRole.Store(reactChannel.Guild.Id, DialogContext.NewRole.Id, reaction.UserId);
                await roleCreatedMessage.ModifyAsync(m => {
                    DialogContext.UserAction = DialogContext.Action.SetupMute;
                    m.Content = DialogContext.Filter();
                });
                return true;
            }
            //Fix Permissions for the role
            else if (TakeAction == takeAction.FixPermissions) {
                IUserMessage roleCreatedMessage = await reaction.Channel.SendMessageAsync("Fixing Permissions...");
                foreach (IGuildChannel channel in reactChannel.Guild.Channels)
                    if (channel is ITextChannel) await channel.AddPermissionOverwriteAsync(DialogContext.NewRole, muteChannelPerms);
                await roleCreatedMessage.ModifyAsync(m => {
                    DialogContext.Error = Program.Dialogs.Message.Setup.Mute.CorrectedPermissions;
                    m.Content = DialogContext.Filter();
                });
            }
            return false;
        }
    }
}