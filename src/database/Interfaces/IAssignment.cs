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
		public AssignmentType AssignmentType { get; private set; }
		public ulong GuildId { get; private set; }
		public ulong ChannelId { get; private set; }
		public ulong MessageId { get; private set; }
		public ulong UserId { get; private set; }
		public DateTime SetOff { get; private set; }
		public DateTime SetAt { get; private set; }
		public string Content { get; private set; }
		public int AssignmentId { get; private set; }
	}
}
