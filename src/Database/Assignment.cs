using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Tomoe.Db
{
	public enum AssignmentType
	{
		Reminder,
		Tempban,
		Tempmute,
		Tempantimeme,
		Tempvoiceban
	}

	public class Assignment
	{
		[Key]
		public int Id { get; internal set; } = RandomNumberGenerator.GetInt32(int.MaxValue);
		public AssignmentType AssignmentType { get; internal set; }
		public ulong GuildId { get; internal set; }
		public ulong ChannelId { get; internal set; }
		public ulong MessageId { get; internal set; }
		public ulong UserId { get; internal set; }
		public DateTime SetOff { get; internal set; }
		public DateTime SetAt { get; internal set; } = DateTime.Now;
		public string Content { get; internal set; }
	}
}
