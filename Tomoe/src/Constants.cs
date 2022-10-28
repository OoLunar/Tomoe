using System.Linq;
using DSharpPlus.Entities;

namespace Tomoe
{
    public class Constants
    {
        public const string MissingReason = "Notice: No reason was provided.";
        public const string RawEmbed = "Error: Cannot get the raw version of an embed!";
        public const string GuildCommand = "Error: This command can only be used in a guild!";

        // Not constants, but they got no where else to go.
        public static readonly DiscordEmoji ThumbsUp = DiscordEmoji.FromUnicode(Program.Client.ShardClients.Values.First(), "👍");
        public static readonly DiscordEmoji ThumbsDown = DiscordEmoji.FromUnicode(Program.Client.ShardClients.Values.First(), "👎");

        // TODO: Grab these from config
        public static readonly DiscordEmoji Loading = DiscordEmoji.FromGuildEmote(Program.Client.ShardClients.Values.First(), 844773853636460575);

        public static readonly DiscordEmoji Check = DiscordEmoji.FromUnicode(Program.Client.ShardClients.Values.First(), "✅");
        public static readonly DiscordEmoji Failed = DiscordEmoji.FromUnicode(Program.Client.ShardClients.Values.First(), "❌");
        public static readonly DiscordEmoji QuestionMark = DiscordEmoji.FromUnicode(Program.Client.ShardClients.Values.First(), "❓");
    }
}
