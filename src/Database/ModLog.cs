using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Tomoe.Db
{
	public class ModLog
	{
		[Key]
		public int Id { get; internal set; }
		public int LogId { get; internal set; }
		public ulong GuildId { get; internal set; }
		public ulong ChannelId { get; internal set; }
		public ulong MessageId { get; internal set; }
		public string JumpLink { get; internal set; }
		public string Reason { get; internal set; }
		public string Action { get; internal set; }
	}
}
