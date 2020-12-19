using System;

namespace Tomoe.Database.Interfaces {
    // Better not be faking them.
    public interface ITasks {
        void Create(TaskType taskType, ulong guildId, ulong channelId, ulong userId, DateTime setOff, DateTime setAt, string content);
        void Remove(ulong userId, DateTime setAt, DateTime setOff, TaskType taskType);
        Task[] Select(ulong userId, TaskType taskType);
        Task[] SelectAllTasks();
        Task[] SelectAllReminders(ulong userId);
    }

    public enum TaskType {
        Reminder,
        TempBan,
        TempMute,
        TempNoMeme,
        TempNoVC
    }

    public struct Task {
        public TaskType TaskType;
        public ulong GuildId;
        public ulong ChannelId;
        public ulong UserId;
        public DateTime SetOff;
        public DateTime SetAt;
        public string Content;
    }
}