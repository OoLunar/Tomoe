using System;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class CommandUsage
	{
		[Key]
		public int Id { get; internal set; }
		public string JumpLink { get; internal set; }
		public string Command { get; internal set; }
		public string Executer { get; internal set; }
		public ulong ExecuterId { get; internal set; }
		public DateTime When { get; internal set; } = DateTime.UtcNow;
	}
}
