using System;

namespace Tomoe.Database.Interfaces
{
	public struct Strike
	{
		public ulong GuildId { get; set; }
		public ulong VictimId { get; set; }
		public ulong IssuerId { get; set; }
		public string[] Reason { get; set; }
		public string JumpLink { get; set; }
		public bool VictimMessaged { get; set; }
		public bool Dropped { get; set; }
		public DateTime CreatedAt { get; set; }
		public int Id { get; set; }
		public int StrikeCount { get; set; }
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
