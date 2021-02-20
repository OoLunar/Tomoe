using System.Collections.Generic;

namespace Tomoe.Db
{
	public class Tag
	{
		public ulong GuildId { get; internal set; }
		public ulong OwnerId { get; internal set; }
		public Tag OriginalTag { get; internal set; } = null;
		public List<string> Aliases { get; internal set; }
		public string Name { get; internal set; }
		public string Content { get; internal set; }
		public int Uses { get; internal set; }
	}
}
