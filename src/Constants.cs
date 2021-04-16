using DSharpPlus.Entities;

namespace Tomoe
{
	public class Constants
	{
		public const string UserNotFound = "**[Error: User not found.]**";
		public const string RawEmbed = "**[Error: Cannot get the raw version of an embed!]**";
		public const string MissingPermissions = "**[Denied: Missing permissions.]**";
		public const string NotAGuild = "**[Denied: Guild command.]**";

		// Not constants, but they got no where else to go.
		public static readonly DiscordEmoji ThumbsUp = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "👍");
		public static readonly DiscordEmoji ThumbsDown = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "👎");
		public static readonly DiscordEmoji Loading = DiscordEmoji.FromGuildEmote(Program.Client.ShardClients[0], 819460200141553704);
		public static readonly DiscordEmoji Check = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "✅");
		public static readonly DiscordEmoji Failed = DiscordEmoji.FromUnicode(Program.Client.ShardClients[0], "❌");
	}
}
