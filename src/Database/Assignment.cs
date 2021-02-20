using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Tomoe.Db
{
	public enum AssignmentType
	{
		Reminder,
		TempBan,
		TempMute,
		TempAntimeme,
		TempVoiceBan
	}

	public class Assignment
	{
		[Key]
		public int AssignmentId { get; set; } = RandomNumberGenerator.GetInt32(int.MaxValue);
		public AssignmentType AssignmentType { get; set; }
		public ulong GuildId { get; set; }
		public ulong ChannelId { get; set; }
		public ulong MessageId { get; set; }
		public ulong UserId { get; set; }
		public DateTime SetOff { get; set; }
		public DateTime SetAt { get; set; } = DateTime.Now;
		public string Content { get; set; }
	}
}
