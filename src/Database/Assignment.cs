using System;
using System.ComponentModel.DataAnnotations;

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
		public int Id { get; internal set; }
		public AssignmentType Type { get; internal set; }
		public ulong GuildId { get; internal set; }
		public ulong ChannelId { get; internal set; }
		public ulong MessageId { get; internal set; }
		public ulong UserId { get; internal set; }
		public DateTime SetOff { get; internal set; }
		public DateTime SetAt { get; internal set; } = DateTime.Now;
		public string Content { get; internal set; }
		public Uri Jumplink { get; internal set; }
		public bool IsDm { get; internal set; }
		public bool IsStatic { get; internal set; }
		public TimeSpan Annuality { get; internal set; }
	}
}
