using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Tomoe.Utils {
    public static class DialogContextExtensions {
        public static string Filter(this DialogContext context) {
            if (DialogContext.GetActionType(context.UserAction)) {
                // We send an embed
                return null;
            } else {
                // We send a message
                string modifyString = DialogContext.DialogMessage[context.UserAction];
                if (context.Error != null) modifyString = context.Error;
                IGuildUser guildIssuer;
                IGuildUser guildVictim;
                if (context.Guild != null) modifyString = modifyString
                    .Replace("$guild_name", context.Guild.Name)
                    .Replace("$guild_id", context.Guild.Id.ToString());
                if (context.Issuer != null) {
                    // Get the user in the guild
                    guildIssuer = AsyncHelpers.RunSync<IGuildUser>(() => context.Guild.GetUserAsync(context.Issuer.Id));
                    if (guildIssuer != null) modifyString = modifyString.Replace("$issuer_nick", guildIssuer.GetCommonName());
                    // issuer isn't in the guild, replace nickname with username since GetCommonName will fail.
                    else {
                        context.Issuer = AsyncHelpers.RunSync<Discord.Rest.RestUser>(() => Program.Client.Rest.GetUserAsync(context.Issuer.Id));
                        modifyString = modifyString.Replace("$issuer_nick", context.Issuer.Username);
                    }
                    modifyString = modifyString
                        .Replace("$issuer_id", context.Issuer.Id.ToString())
                        .Replace("$issuer_username", context.Issuer.Username)
                        .Replace("$issuer_discriminator", context.Issuer.Discriminator)
                        .Replace("$issuer", context.Issuer.Mention);
                }
                if (context.Victim != null) {
                    // Get the user in the guild
                    guildVictim = AsyncHelpers.RunSync<IGuildUser>(() => context.Guild.GetUserAsync(context.Victim.Id));
                    if (guildVictim != null) modifyString = modifyString.Replace("$victim_nick", guildVictim.GetCommonName());
                    // Victim isn't in the guild, replace nickname with username since GetCommonName will fail.
                    else {
                        context.Victim = AsyncHelpers.RunSync<Discord.Rest.RestUser>(() => Program.Client.Rest.GetUserAsync(context.Victim.Id));
                        modifyString = modifyString.Replace("$victim_nick", context.Victim.Username);
                    }
                    modifyString = modifyString
                        .Replace("$victim_id", context.Victim.Id.ToString())
                        .Replace("$victim_username", context.Victim.Username)
                        .Replace("$victim_discriminator", context.Victim.Discriminator)
                        .Replace("$victim", context.Victim.Mention);
                }
                if (context.NewRole != null) modifyString = modifyString
                    .Replace("$new_role_name", context.NewRole.Name)
                    .Replace("$new_role_id", context.NewRole.Id.ToString())
                    .Replace("$new_role", context.NewRole.Mention);
                if (context.OldRole != null) modifyString = modifyString
                    .Replace("$old_role_name", context.OldRole.Name)
                    .Replace("$old_role_id", context.OldRole.Id.ToString())
                    .Replace("$old_role", context.OldRole.Mention);
                if (context.Reason != null) {
                    // Filter the Reason
                    while (Regex.IsMatch(context.Reason, @"<@&(\d+)>", RegexOptions.Multiline) || Regex.IsMatch(context.Reason, @"<@!?(\d+)>", RegexOptions.Multiline) || context.Reason.Contains("@everyone") || context.Reason.Contains("@here")) {
                        //Replaces all mentions with ID's
                        string mentionNickID = Regex.Match(context.Reason, @"<@!?(\d+)>", RegexOptions.Multiline).Groups[1].Value;
                        mentionNickID = $"`{mentionNickID}` (User {AsyncHelpers.RunSync<IGuildUser>(() => context.Guild.GetUserAsync(ulong.Parse(mentionNickID))).GetCommonName()})";
                        context.Reason = Regex.Replace(context.Reason, @"<@!?\d+>", mentionNickID);
                        string roleID = Regex.Match(context.Reason, @"<@&(\d+)>", RegexOptions.Multiline).Groups[1].Value;
                        roleID = $"`{roleID}` (Role {context.Guild.GetRole(ulong.Parse(roleID)).Name})";
                        context.Reason = Regex.Replace(context.Reason, @"<@&\d+>", roleID);
                        context.Reason = context.Reason.Replace("@everyone", $"`{context.Guild.Id}` (Failed ping everyone attempt)").Replace("@here", $"`{context.Guild.Id}` (Failed ping here attempt)");
                    }
                    modifyString = modifyString.Replace("$reason", context.Reason);
                } else {
                    modifyString = modifyString.Replace("$reason", "**[No reason was provided.]**");
                }
                if (context.RequiredGuildPermission.ToString() != null)
                    modifyString = modifyString.Replace("$required_guild_permission", context.RequiredGuildPermission.ToString());
                if (context.Timestamp != null)
                    modifyString = modifyString.Replace("$timestamp", context.Timestamp.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss"));
                else
                    modifyString = modifyString.Replace("$timestamp", new System.DateTimeOffset().ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss"));
                modifyString = modifyString
                    .Replace("$past_action", context.UserAction.ToPastString())
                    .Replace("$action", context.UserAction.ToString())
                    .Replace("\\n", "\n");
                return modifyString;
            }
        }

        public static async Task<Discord.Rest.RestUserMessage> SendChannel(this DialogContext context) { return await (context.Channel as ISocketMessageChannel).SendMessageAsync(context.Filter()); }
        public static async Task<Discord.IUserMessage> SendDM(this DialogContext context) { return await (await context.Victim.GetOrCreateDMChannelAsync()).SendMessageAsync(context.Filter()); }
    }

    public class DialogContext {
        public IUser Issuer;
        public IUser Victim;
        public IRole OldRole;
        public IRole NewRole;
        public string Error;
        public string Reason;
        public IGuild Guild;
        public IChannel Channel;
        public Action UserAction;
        public GuildPermission RequiredGuildPermission;
        public System.DateTimeOffset Timestamp;
        public IMessage OldMessage;
        public IMessage NewMessage;

        public DialogContext() { }

        public static Dictionary<Action, bool> DialogFormat = new Dictionary<Action, bool>() { { Tomoe.Utils.DialogContext.Action.Ban, Program.Dialogs.Format.Action.Ban }, { Tomoe.Utils.DialogContext.Action.Kick, Program.Dialogs.Format.Action.Kick }, { Tomoe.Utils.DialogContext.Action.Mute, Program.Dialogs.Format.Action.Mute }, { Tomoe.Utils.DialogContext.Action.NoMeme, Program.Dialogs.Format.Action.NoMeme }, { Tomoe.Utils.DialogContext.Action.Strike, Program.Dialogs.Format.Action.Strike }, { Tomoe.Utils.DialogContext.Action.TempBan, Program.Dialogs.Format.Action.TempBan }, { Tomoe.Utils.DialogContext.Action.TempMute, Program.Dialogs.Format.Action.TempMute }, { Tomoe.Utils.DialogContext.Action.TempNoMeme, Program.Dialogs.Format.Action.TempNoMeme }, { Tomoe.Utils.DialogContext.Action.UnBan, Program.Dialogs.Format.Action.UnBan }, { Tomoe.Utils.DialogContext.Action.UnMute, Program.Dialogs.Format.Action.UnMute }, { Tomoe.Utils.DialogContext.Action.UnNoMeme, Program.Dialogs.Format.Action.UnNoMeme } };
        public static Dictionary<Action, string> DialogMessage = new Dictionary<Action, string>() { { Tomoe.Utils.DialogContext.Action.Ban, Program.Dialogs.Message.Action.Ban }, { Tomoe.Utils.DialogContext.Action.Kick, Program.Dialogs.Message.Action.Kick }, { Tomoe.Utils.DialogContext.Action.Mute, Program.Dialogs.Message.Action.Mute }, { Tomoe.Utils.DialogContext.Action.NoMeme, Program.Dialogs.Message.Action.NoMeme }, { Tomoe.Utils.DialogContext.Action.Strike, Program.Dialogs.Message.Action.Strike }, { Tomoe.Utils.DialogContext.Action.TempBan, Program.Dialogs.Message.Action.TempBan }, { Tomoe.Utils.DialogContext.Action.TempMute, Program.Dialogs.Message.Action.TempMute }, { Tomoe.Utils.DialogContext.Action.TempNoMeme, Program.Dialogs.Message.Action.TempNoMeme }, { Tomoe.Utils.DialogContext.Action.UnBan, Program.Dialogs.Message.Action.UnBan }, { Tomoe.Utils.DialogContext.Action.UnMute, Program.Dialogs.Message.Action.UnMute }, { Tomoe.Utils.DialogContext.Action.UnNoMeme, Program.Dialogs.Message.Action.UnNoMeme } };

        public enum Action {
            Ban,
            Kick,
            Mute,
            NoMeme,
            Strike,
            SetupMute,
            SetupNoMeme,
            UnBan,
            UnMute,
            UnNoMeme,
            TempBan,
            TempMute,
            TempNoMeme
        }

        ///<summary>Returns true if it's an embed, returns false if it's a message.</summary>
        public static bool GetActionType(Action action) {
            bool returnValue = false;
            DialogFormat.TryGetValue(action, out returnValue);
            return returnValue;
        }
    }
}