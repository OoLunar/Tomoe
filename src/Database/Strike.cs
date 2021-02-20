using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class Strike
	{
		[Key]
		public int StrikeId { get; set; }
		public ulong GuildId { get; set; }
		public ulong IssuerId { get; set; }
		public ulong VictimId { get; set; }
		public List<string> Reason { get; set; }
		public Uri JumpLink { get; set; }
		public bool VictimMessaged { get; set; }
		public bool Dropped { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
