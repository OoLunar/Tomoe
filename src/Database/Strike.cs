using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class Strike
	{
		[Key]
		public int Id { get; internal set; }
		public ulong GuildId { get; internal set; }
		public int CaseId { get; internal set; }
		public ulong IssuerId { get; internal set; }
		public ulong VictimId { get; internal set; }
		public List<string> Reasons { get; internal set; } = new();
		public Uri JumpLink { get; internal set; }
		public bool VictimMessaged { get; internal set; }
		public bool Dropped { get; internal set; }
		public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;
	}
}
