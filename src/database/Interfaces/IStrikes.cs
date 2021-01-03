using System;

namespace Tomoe.Database.Interfaces
{
	public struct Strike
	{
		public ulong GuildId { get; private set; }
		public ulong VictimId { get; private set; }
		public ulong IssuerId { get; private set; }
		public string[] Reason { get; private set; }
		public string JumpLink { get; private set; }
		public bool VictimMessaged { get; private set; }
		public bool Dropped { get; private set; }
		public DateTime CreatedAt { get; private set; }
		public int Id { get; private set; }
		public int StrikeCount { get; private set; }
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
