using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Tomoe.Utils.Dialog {
    public static class Extensions {
        public static string ToPastString(this Context.Action action) {
            switch (action) {
                case Tomoe.Utils.Dialog.Context.Action.Ban:
                    return "banned";
                case Tomoe.Utils.Dialog.Context.Action.Kick:
                    return "kicked";
                case Tomoe.Utils.Dialog.Context.Action.Mute:
                    return "muted";
                case Tomoe.Utils.Dialog.Context.Action.NoMeme:
                    return "no memed";
                case Tomoe.Utils.Dialog.Context.Action.Strike:
                    return "striken";
                case Tomoe.Utils.Dialog.Context.Action.TempBan:
                    return "temporarily banned";
                case Tomoe.Utils.Dialog.Context.Action.TempMute:
                    return "temporarily muted";
                case Tomoe.Utils.Dialog.Context.Action.TempNoMeme:
                    return "temporarily no memed";
                case Tomoe.Utils.Dialog.Context.Action.UnBan:
                    return "unbanned";
                case Tomoe.Utils.Dialog.Context.Action.UnMute:
                    return "unmuted";
                case Tomoe.Utils.Dialog.Context.Action.UnNoMeme:
                    return "no memed no longer";
                default:
                    return action.ToString();
            }
        }

        public static string Filter(this Dialog.Context context) {
            if (Dialog.Context.IsEmbed(context.UserAction))
                // We send an embed
                return null;
            else {
                // We send a message
                string modifyString = Dialog.Context.DialogMessage[context.UserAction];
                if (context.Error != null) modifyString = context.Error;
                IGuildUser guildIssuer;
                IGuildUser guildVictim;
                if (context.Guild != null) modifyString = modifyString
                    .Replace("$guild_name", context.Guild.Name)
                    .Replace("$guild_id", context.Guild.Id.ToString());
                if (context.Issuer != null) {
                    // Get the user in the guild
                    guildIssuer = context.Guild.GetUserAsync(context.Issuer.Id).GetAwaiter().GetResult();
                    if (guildIssuer != null) modifyString = modifyString.Replace("$issuer_nick", guildIssuer.GetCommonName());
                    // issuer isn't in the guild, replace nickname with username since GetCommonName will fail.
                    else {
                        context.Issuer = Program.Client.Rest.GetUserAsync(context.Issuer.Id).GetAwaiter().GetResult();
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
                    guildVictim = context.Guild.GetUserAsync(context.Victim.Id).GetAwaiter().GetResult();
                    if (guildVictim != null) modifyString = modifyString.Replace("$victim_nick", guildVictim.GetCommonName());
                    // Victim isn't in the guild, replace nickname with username since GetCommonName will fail.
                    else {
                        context.Victim = Program.Client.Rest.GetUserAsync(context.Victim.Id).GetAwaiter().GetResult();
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
                        // Replaces all mentions with ID's
                        string mentionNickID = Regex.Match(context.Reason, @"<@!?(\d+)>", RegexOptions.Multiline).Groups[1].Value;
                        mentionNickID = $"`{mentionNickID}` (User {context.Guild.GetUserAsync(ulong.Parse(mentionNickID)).GetAwaiter().GetResult().GetCommonName()})";
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
                if (context.OldMessage != null) modifyString = modifyString.Replace("$old_message", context.OldMessage.Content).Replace("$message_id", context.OldMessage.Id.ToString());
                if (context.NewMessage != null) modifyString = modifyString.Replace("$new_message", context.NewMessage.Content).Replace("$message_id", context.NewMessage.Id.ToString());
                if (context.Channel != null) modifyString = modifyString
                    .Replace("$channel", $"<#{context.Channel.Id}>")
                    .Replace("$channel_id", context.Channel.Id.ToString())
                    .Replace("$channel_name", context.Channel.Name);

                modifyString = modifyString
                    .Replace("$past_action", context.UserAction.ToPastString())
                    .Replace("$action", context.UserAction.ToString())
                    .Replace("\\n", "\n");
                return modifyString;
            }
        }

        public static async Task<Discord.Rest.RestUserMessage> SendChannel(this Dialog.Context context) => await (context.Channel as ISocketMessageChannel).SendMessageAsync(context.Filter());

        public static async Task<Discord.IUserMessage> SendDM(this Dialog.Context context) {
            context.Error = Tomoe.Utils.Dialog.Context.DMDialogMessage[context.UserAction];
            Discord.IUserMessage returnMessage = await (await context.Victim.GetOrCreateDMChannelAsync()).SendMessageAsync(context.Filter());
            context.Error = null;
            return returnMessage;
        }
    }

    public class Context {
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

        public Context() { }

        public static Dictionary<Action, bool> DialogFormat = new Dictionary<Action, bool>() { { Action.Ban, Program.Dialogs.Format.Action.Ban }, { Action.Kick, Program.Dialogs.Format.Action.Kick }, { Action.Mute, Program.Dialogs.Format.Action.Mute }, { Action.NoMeme, Program.Dialogs.Format.Action.NoMeme }, { Action.Strike, Program.Dialogs.Format.Action.Strike }, { Action.TempBan, Program.Dialogs.Format.Action.TempBan }, { Action.TempMute, Program.Dialogs.Format.Action.TempMute }, { Action.TempNoMeme, Program.Dialogs.Format.Action.TempNoMeme }, { Action.UnBan, Program.Dialogs.Format.Action.UnBan }, { Action.UnMute, Program.Dialogs.Format.Action.UnMute }, { Action.UnNoMeme, Program.Dialogs.Format.Action.UnNoMeme } };
        public static Dictionary<Action, string> DialogMessage = new Dictionary<Action, string>() { { Action.AntiraidBan, Program.Dialogs.Message.Events.AntiraidBan }, { Action.Ban, Program.Dialogs.Message.Action.Ban }, { Action.Kick, Program.Dialogs.Message.Action.Kick }, { Action.Mute, Program.Dialogs.Message.Action.Mute }, { Action.NoMeme, Program.Dialogs.Message.Action.NoMeme }, { Action.Strike, Program.Dialogs.Message.Action.Strike }, { Action.TempBan, Program.Dialogs.Message.Action.TempBan }, { Action.TempMute, Program.Dialogs.Message.Action.TempMute }, { Action.TempNoMeme, Program.Dialogs.Message.Action.TempNoMeme }, { Action.UnBan, Program.Dialogs.Message.Action.UnBan }, { Action.UnMute, Program.Dialogs.Message.Action.UnMute }, { Action.UnNoMeme, Program.Dialogs.Message.Action.UnNoMeme }, { Action.SetupMute, Program.Dialogs.Message.Setup.Mute.AlreadySetup }, { Action.SetupNoMeme, Program.Dialogs.Message.Setup.NoMeme.AlreadySetup } };

        public static Dictionary<Action, bool> DMDialogFormat = new Dictionary<Action, bool>() { { Action.Ban, Program.Dialogs.Format.Dm.Ban }, { Action.Kick, Program.Dialogs.Format.Dm.Kick }, { Action.Mute, Program.Dialogs.Format.Dm.Mute }, { Action.NoMeme, Program.Dialogs.Format.Dm.NoMeme }, { Action.Strike, Program.Dialogs.Format.Dm.Strike }, { Action.TempBan, Program.Dialogs.Format.Dm.TempBan }, { Action.TempMute, Program.Dialogs.Format.Dm.TempMute }, { Action.TempNoMeme, Program.Dialogs.Format.Dm.TempNoMeme }, { Action.UnBan, Program.Dialogs.Format.Dm.UnBan }, { Action.UnMute, Program.Dialogs.Format.Dm.UnMute }, { Action.UnNoMeme, Program.Dialogs.Format.Dm.UnNoMeme } };
        public static Dictionary<Action, string> DMDialogMessage = new Dictionary<Action, string>() { { Action.AntiraidBan, Program.Dialogs.Message.Dm.Antiraid }, { Action.Ban, Program.Dialogs.Message.Dm.Ban }, { Action.Kick, Program.Dialogs.Message.Dm.Kick }, { Action.Mute, Program.Dialogs.Message.Dm.Mute }, { Action.NoMeme, Program.Dialogs.Message.Dm.NoMeme }, { Action.Strike, Program.Dialogs.Message.Dm.Strike }, { Action.TempBan, Program.Dialogs.Message.Dm.TempBan }, { Action.TempMute, Program.Dialogs.Message.Dm.TempMute }, { Action.TempNoMeme, Program.Dialogs.Message.Dm.TempNoMeme }, { Action.UnBan, Program.Dialogs.Message.Dm.UnBan }, { Action.UnMute, Program.Dialogs.Message.Dm.UnMute }, { Action.UnNoMeme, Program.Dialogs.Message.Dm.UnNoMeme } };
        public static Dictionary<Action, dynamic> DMDialogEmbeds = new Dictionary<Action, dynamic>() { { Tomoe.Utils.Dialog.Context.Action.Ban, Program.Dialogs.Embed.Dm.Ban }, { Tomoe.Utils.Dialog.Context.Action.Kick, Program.Dialogs.Embed.Dm.Kick }, { Tomoe.Utils.Dialog.Context.Action.Mute, Program.Dialogs.Embed.Dm.Mute }, { Tomoe.Utils.Dialog.Context.Action.NoMeme, Program.Dialogs.Embed.Dm.NoMeme }, { Tomoe.Utils.Dialog.Context.Action.Strike, Program.Dialogs.Embed.Dm.Strike }, { Tomoe.Utils.Dialog.Context.Action.TempBan, Program.Dialogs.Embed.Dm.TempBan }, { Tomoe.Utils.Dialog.Context.Action.TempMute, Program.Dialogs.Embed.Dm.TempMute }, { Tomoe.Utils.Dialog.Context.Action.TempNoMeme, Program.Dialogs.Embed.Dm.TempNoMeme }, { Tomoe.Utils.Dialog.Context.Action.UnBan, Program.Dialogs.Embed.Dm.UnBan }, { Tomoe.Utils.Dialog.Context.Action.UnMute, Program.Dialogs.Embed.Dm.UnMute }, { Tomoe.Utils.Dialog.Context.Action.UnNoMeme, Program.Dialogs.Embed.Dm.UnNoMeme } };

        public enum Action {
            AntiraidBan,
            Ban,
            Kick,
            Mute,
            NoMeme,
            Pardon,
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
        public static bool IsEmbed(Action action) {
            bool returnValue = false;
            DialogFormat.TryGetValue(action, out returnValue);
            return returnValue;
        }
    }
}