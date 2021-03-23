using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class ReactionRole
	{
		[Key]
		public ulong MessageId { get; internal set; }
		public ulong RoleId { get; internal set; }
		public ulong EmojiId { get; internal set; }
		public string EmojiName { get; internal set; }
		public bool IsDefaultEmoji { get; internal set; }
	}
}
