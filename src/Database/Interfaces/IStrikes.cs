using System;

namespace Tomoe.Database.Interfaces
{
	public struct Strike
	{
		public ulong GuildId { get; internal set; }
		public ulong VictimId { get; internal set; }
		public ulong IssuerId { get; internal set; }
		public string[] Reason { get; internal set; }
		public string JumpLink { get; internal set; }
		public bool VictimMessaged { get; internal set; }
		public bool Dropped { get; internal set; }
		public DateTime CreatedAt { get; internal set; }
		public int Id { get; internal set; }
		public int StrikeCount { get; internal set; }
	}

	public interface IStrikes : IDisposable
	{
		Strike? Retrieve(int strikeId);
		Strike[] GetVictim(ulong guildId, ulong victimId);
		Strike[] GetIssued(ulong guildId, ulong issuerId);
		Strike? Add(ulong guildId, ulong victimId, ulong issuerId, string reason, string jumpLink, bool victimMessaged);
		Strike? Drop(int strikeId, string reason);
		void Edit(int strikeId, string reason);
	}
}
