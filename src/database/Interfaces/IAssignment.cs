using System;

namespace Tomoe.Database.Interfaces
{
	public interface IAssignment : IDisposable
	{
		void Create(AssignmentType taskType, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime setOff, DateTime setAt, string content);
		void Remove(int taskId);
		Assignment[] Retrieve(ulong userId, AssignmentType taskType);
		Assignment? Retrieve(ulong userId, AssignmentType taskType, int taskId);
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
		public AssignmentType AssignmentType { get; internal set; }
		public ulong GuildId { get; internal set; }
		public ulong ChannelId { get; internal set; }
		public ulong MessageId { get; internal set; }
		public ulong UserId { get; internal set; }
		public DateTime SetOff { get; internal set; }
		public DateTime SetAt { get; internal set; }
		public string Content { get; internal set; }
		public int AssignmentId { get; internal set; }
	}
}
