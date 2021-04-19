using System;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class ReactionRole
	{
		[Key]
		public ulong GuildId { get; internal set; }
		public ulong MessageId { get; internal set; }
		public ulong RoleId { get; internal set; }
		public string EmojiName { get; internal set; }
	}
}
