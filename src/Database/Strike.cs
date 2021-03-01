using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

namespace Tomoe.Db
{
	public class Strike
	{
		[Key]
		public int Id { get; internal set; }
		public ulong GuildId { get; internal set; }
		public ulong IssuerId { get; internal set; }
		public ulong VictimId { get; internal set; }
		public List<string> Reasons { get; internal set; } = new();
		public Uri JumpLink { get; internal set; }
		public bool VictimMessaged { get; internal set; }
		public bool Dropped { get; internal set; }
		public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;

		// Added 1 to the end because we don't want a Case #0
		public Strike(ulong guildId) => Id = (Program.ServiceProvider.CreateScope().ServiceProvider.GetService(typeof(Database)) as Database).Strikes.Count(strike => strike.GuildId == guildId) + 1;
	}
}
