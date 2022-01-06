using DSharpPlus.Entities;

namespace Tomoe
{
    public class Constants
    {
        public const string MissingReason = "**[Notice: No reason was provided.]**";
        public const string SelfPunishment = "**[Warning: You're about to punish yourself. Do you still want to go through with this?]**";
        public const string MissingRole = "**[Error: No role has been set!]**";
        public const string UserNotFound = "**[Error: User not found.]**";
        public const string RawEmbed = "**[Error: Cannot get the raw version of an embed!]**";
        public const string GuildOwner = "**[Denied: Cannot execute actions on the guild owner!]**";
        public const string MissingPermissions = "**[Denied: Missing permissions.]**";
        public const string NotAGuild = "**[Denied: Guild command.]**";
        public const string SelfBotAction = "**[Denied: Cannot execute on myself.]**";
        public const string Hierarchy = "**[Denied: Prevented by hierarchy.]**";
        public const string GuildNotInDatabase = "**[Error: Failed to execute command because the guild is not in the database!]**";
        public const string Version = "1.0.0-beta2.4";

        // Not constants, but they got no where else to go.
        public static readonly DiscordEmoji ThumbsUp = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "👍");
        public static readonly DiscordEmoji ThumbsDown = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "👎");
        public static readonly DiscordEmoji Loading = DiscordEmoji.FromGuildEmote(Program.Client.ShardClients[0], 844773853636460575);
        public static readonly DiscordEmoji NoPermission = DiscordEmoji.FromGuildEmote(Program.Client.ShardClients[0], 844773889355415573);
        public static readonly DiscordEmoji Check = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "✅");
        public static readonly DiscordEmoji Failed = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "❌");
        public static readonly DiscordEmoji QuestionMark = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "❓");
    }
}