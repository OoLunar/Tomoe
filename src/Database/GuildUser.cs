using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class GuildUser
	{
		[Key]
		public ulong Id { get; internal set; }
		public List<ulong> Roles { get; internal set; } = new();
		public List<Strike> Strikes { get; internal set; } = new();
		public bool IsMuted { get; internal set; }
		public bool IsAntimemed { get; internal set; }
		public bool IsVoiceBanned { get; internal set; }

		public GuildUser(ulong id) => Id = id;
	}
}
