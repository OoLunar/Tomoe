using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class User
	{
		[Key]
		public ulong UserId { get; internal set; }
		public List<ulong> GuildIds { get; internal set; }
	}
}
