using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class Guild
	{
		[Key]
		public ulong Id { get; internal set; }
		public List<ReactionRole> ReactionRoles { get; private set; } = new();

		public Guild(ulong id) => Id = id;
	}
}
