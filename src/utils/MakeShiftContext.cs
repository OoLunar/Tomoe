using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Tomoe.Utils
{
	internal class MakeShiftContext
	{
		private static readonly Logger Logger = new Logger("Utils/MakeShiftContext");
		public DiscordGuild Guild;
		public DiscordChannel Channel;
		public DiscordMember Member;
		public DiscordUser User;
		public DiscordMessage Message;
		public DiscordShardedClient Client = Program.Client;

		public MakeShiftContext(ulong guildId, ulong channelId, ulong messageId, ulong userId)
		{
			DiscordClient shard = Client.GetShard(guildId);
			Logger.Trace($"Getting user {userId}");
			User = shard.GetUserAsync(userId).ConfigureAwait(false).GetAwaiter().GetResult();
			Logger.Trace($"Getting guild {guildId}");
			Guild = shard.GetGuildAsync(guildId).ConfigureAwait(false).GetAwaiter().GetResult();
			Logger.Trace($"Getting channel {userId} from guild {guildId}");
			Channel = Guild.GetChannel(channelId);
			Logger.Trace($"Getting member {userId} from guild {guildId}");
			Member = Guild.GetMemberAsync(userId).ConfigureAwait(false).GetAwaiter().GetResult();
			Logger.Trace($"Getting message {messageId} from channel {channelId} from guild {guildId}");
			Message = Channel.GetMessageAsync(messageId).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Quickly respond to the message that triggered the command.
		/// </summary>
		/// <param name="content">Message to respond with.</param>
		/// <param name="isTTS">Whether the message is to be spoken aloud.</param>
		/// <param name="embed">Embed to attach.</param>
		/// <param name="mentions">A list of mentions permitted to trigger a ping.</param>
		/// <returns></returns>
		public Task<DiscordMessage> RespondAsync(string content = null, bool isTTS = false, DiscordEmbed embed = null, IEnumerable<IMention> mentions = null) => this.Message.RespondAsync(content, isTTS, embed, mentions);
	}
}
