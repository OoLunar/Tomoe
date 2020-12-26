using System;

namespace Tomoe.Database.Interfaces
{
	public interface IAssignment : IDisposable
	{
		void Create(AssignmentType taskType, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime setOff, DateTime setAt, string content);
		void Remove(int taskId);
		Assignment[] Select(ulong userId, AssignmentType taskType);
		Assignment? Select(ulong userId, AssignmentType taskType, int taskId);
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
		public AssignmentType TaskType;
		public ulong GuildId;
		public ulong ChannelId;
		public ulong MessageId;
		public ulong UserId;
		public DateTime SetOff;
		public DateTime SetAt;
		public string Content;
		public int TaskId;
	}
}
