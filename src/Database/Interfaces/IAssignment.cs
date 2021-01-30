using System;

namespace Tomoe.Database.Interfaces
{
	public interface IAssignment : IDisposable
	{
		void Create(AssignmentType assignmentType, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime setOff, DateTime setAt, string content);
		void Remove(int assignmentId);
		Assignment[] Retrieve(ulong userId, AssignmentType assignmentType);
		Assignment? Retrieve(ulong userId, AssignmentType assignmentType, int assignmentId);
		Assignment[] SelectAllAssignments();
		Assignment[] SelectAllReminders(ulong userId);
	}

	public enum AssignmentType
	{
		Reminder,
		TempBan,
		TempMute,
		TempNoMeme,
		TempNoVC
	}

	public struct Assignment
	{
		public AssignmentType AssignmentType { get; set; }
		public ulong GuildId { get; set; }
		public ulong ChannelId { get; set; }
		public ulong MessageId { get; set; }
		public ulong UserId { get; set; }
		public DateTime SetOff { get; set; }
		public DateTime SetAt { get; set; }
		public string Content { get; set; }
		public int AssignmentId { get; set; }
	}
}
