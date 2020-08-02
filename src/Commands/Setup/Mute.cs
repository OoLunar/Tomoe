using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Tomoe.Utils;
using Tomoe.Utils.Cache;

namespace Tomoe.Commands.Setup {
    public class Mute : InteractiveBase {
        public static Dictionary<string, string[]> MuteSetupDialogs = Program.Dialogs.MuteSetup;

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
            if (currentRole == null) {
                MutedRole.Store(Context.Guild.Id, newRole.Id, Context.User.Id);
                foreach (IGuildChannel channel in Context.Guild.Channels) {
                    if (channel is ITextChannel)
                        await channel.AddPermissionOverwriteAsync(newRole, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                }
                await ReplyAsync(MuteSetupDialogs.GetRandomValue("success_setup"));
            } else if (currentRole.RoleID == newRole.Id) {
                await ReplyAsync(MuteSetupDialogs.GetRandomValue("same_role").Replace("$role", Context.Guild.GetRole(currentRole.RoleID).Mention));
            } else {
                IUserMessage sendMessage = await Context.Channel.SendMessageAsync($"{MuteSetupDialogs.GetRandomValue("is_setup").Replace("$administrator", Context.Guild.GetUser(currentRole.UserID).Mention).Replace("$role", Context.Guild.GetRole(currentRole.RoleID).Mention)} Continue anyways?");
                Emoji[] reactions = { new Emoji("\u2611"), new Emoji("\u274C") };
                OverrideRoleCallback roleCallback = new OverrideRoleCallback();
                roleCallback.OldRole = currentRole;
                roleCallback.NewRole = newRole;
                Interactive.AddReactionCallback(sendMessage, roleCallback);
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
        public async Task SetupMuteByID(ulong role) => await SetupMuteByRole(Context.Guild.GetRole(role));

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
            MutedRole roleSet = MutedRole.Get(Context.Guild.Id);
            if (roleSet == null || Context.Guild.GetRole(roleSet.RoleID) == null) {
                IUserMessage sendMessage = await Context.Channel.SendMessageAsync(MuteSetupDialogs.GetRandomValue("not_setup"));
                Emoji[] reactions = { new Emoji("\u2611"), new Emoji("\u274C") };
                Interactive.AddReactionCallback(sendMessage, new SetupRoleCallback());
                await sendMessage.AddReactionsAsync(reactions);
            } else {
                var user = Context.Guild.GetUser(roleSet.UserID);
                string isSetupDialog = Program.Dialogs.MuteSetup.GetRandomValue("is_setup").Replace("$administrator", Context.Guild.GetUser(roleSet.UserID).Mention).Replace("$role", Context.Guild.GetRole(roleSet.RoleID).Mention);
                await ReplyAsync(isSetupDialog);
            }
        }
    }

    /// <summary>The class that handles reactions for <see cref="Tomoe.Commands.Setup.Mute.CheckMuteRole()"/></summary>
    class SetupRoleCallback : IReactionCallback {
        public SocketCommandContext Context { get; }
        public RunMode RunMode => RunMode.Async;
        public ICriterion<SocketReaction> Criterion => new EmptyCriterion<SocketReaction>();
        public TimeSpan? Timeout => new PaginatedMessage().Options.Timeout;

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
            SocketGuildChannel reactChannel = reaction.Channel as SocketGuildChannel;
            if (reaction.Emote.Name == "☑") {
                IUserMessage roleCreatedMessage = await reaction.Channel.SendMessageAsync("Creating role...");
                IRole role = await reactChannel.Guild.CreateRoleAsync("Muted", GuildPermissions.None, Color.DarkGrey, false, false, null);
                await roleCreatedMessage.ModifyAsync(m => { m.Content = "Applying permissions..."; });
                foreach (IGuildChannel channel in reactChannel.Guild.Channels) {
                    if (channel is ITextChannel) {
                        await channel.RemovePermissionOverwriteAsync(role);
                        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                    }
                }
                MutedRole.Store(reactChannel.Guild.Id, role.Id, reaction.UserId);
                await roleCreatedMessage.ModifyAsync(m => { m.Content = Mute.MuteSetupDialogs.GetRandomValue("success_setup").Replace("$role", role.Mention); });
                return true;
            } else if (reaction.Emote.Name == "❌") {
                await reaction.Channel.SendMessageAsync(Mute.MuteSetupDialogs.GetRandomValue("exit"));
                return true;
            } else {
                await (await reaction.Channel.GetMessageAsync(reaction.MessageId)).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                return false;
            }
        }
    }

    class OverrideRoleCallback : IReactionCallback {
        public SocketCommandContext Context { get; }
        public RunMode RunMode => RunMode.Async;
        public ICriterion<SocketReaction> Criterion => new EmptyCriterion<SocketReaction>();
        public TimeSpan? Timeout => new PaginatedMessage().Options.Timeout;
        public MutedRole OldRole;
        public IRole NewRole;

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
            SocketGuildChannel reactChannel = reaction.Channel as SocketGuildChannel;
            if (reaction.Emote.Name == "☑") {
                foreach (IGuildChannel channel in reactChannel.Guild.Channels) {
                    if (channel is ITextChannel) {
                        await channel.RemovePermissionOverwriteAsync(reactChannel.Guild.GetRole(OldRole.RoleID));
                        await channel.AddPermissionOverwriteAsync(NewRole, new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, sendTTSMessages: PermValue.Deny, embedLinks: PermValue.Deny, attachFiles: PermValue.Deny, mentionEveryone: PermValue.Deny, useExternalEmojis: PermValue.Deny));
                    }
                }
                MutedRole.Store(reactChannel.Guild.Id, NewRole.Id, reaction.UserId);
                await reaction.Channel.SendMessageAsync(Mute.MuteSetupDialogs.GetRandomValue("success_setup").Replace("$role", NewRole.Mention));
                return true;
            } else if (reaction.Emote.Name == "❌") {
                await reaction.Channel.SendMessageAsync(Mute.MuteSetupDialogs.GetRandomValue("exit"));
                return true;
            } else {
                await (await reaction.Channel.GetMessageAsync(reaction.MessageId)).RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                return false;
            }
        }
    }
}