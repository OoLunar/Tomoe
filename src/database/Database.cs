using System;
using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json;

namespace Tomoe.Database {

    using Tomoe.Database.Classes;

    public class Database {
        private static Logger Logger = new Logger("Database");
        private IDatabase database;

        [JsonProperty("postgresql")]
        public Classes.Postgresql Postgresql;

        [JsonProperty("sqlite")]
        public Classes.Sqlite Sqlite;

        [JsonProperty("type")]
        public Classes.DatabaseTypes Type;

        public Database() {
            switch (Program.Config.Database.Type) {
                case DatabaseTypes.SQLite:
                    database = new SQLite();
                    break;
                case DatabaseTypes.PostgresSQL:
                    database = new PostgresSQL();
                    break;
            }
        }
    }
}

namespace Tomoe.Database.Classes {

    /// <summary>The database types available to store information like the settings, tasks and cache.</summary>
    public enum DatabaseTypes {
        /// <summary>SQLite is a relational database management system.</summary>
        SQLite,
        /// <summary>PostgreSQL, also known as Postgres, is a free and open-source relational database management system emphasizing extensibility and SQL compliance.</summary>
        PostgresSQL
    }

    /// <summary>Moderation actions that can be executed either through the Discord client or the bot.</summary>
    /// <remarks>Used in logging.</remarks>
    public enum Action {
        /// <summary>Banned permanently through either the Discord client or through the bot.</summary>
        Ban,
        /// <summary>Kicked through either the Discord client or through the bot.</summary>
        Kick,
        /// <summary>Muted permanently through either the Discord client or through the bot.</summary>
        Mute,
        /// <summary>For when the user has been no memed permanently through either the Discord client or through the bot.</summary>
        NoMeme,
        /// <summary>For when the user has their voice chat privileges revoked permanently through either the Discord client or through the bot.</summary>
        NoVoiceChat,
        /// <summary>Striked through the bot.</summary>
        Strike,
        /// <summary>Pardoned a strike through the bot.</summary>
        Pardon,
        /// <summary>Temporarily banned through the bot.</summary>
        TempBan,
        /// <summary>Temporarily muted through the bot.</summary>
        TempMute,
        /// <summary>For when the user has been no memed temporarily through the bot.</summary>
        TempNoMeme,
        /// <summary>For when the user has their voice chat privileges temporarily revoked through the bot.</summary>
        TempNoVoiceChat,
        /// <summary>Unbanned through either the Discord client or through the bot.</summary>
        UnBan,
        /// <summary>Unmuted through either the Discord client or through the bot.</summary>
        UnMute,
        /// <summary>The user has their meme privileges restored through either the Discord client or through the bot.</summary>
        AllowMeme,
        /// <summary>The user has their voice chat privileges restored through either the Discord client or through the bot.</summary>
        AllowVoiceChat
    }

    /// <summary>Actions that affect the server. These settings can be changed through the Discord client, the bot or through the web dashboard.</summary>
    /// <remarks>Used in logging.</remarks>
    public enum AdminAction {
        /// <summary>Sets the mute role for the server.</summary>
        SetMuteRole,
        /// <summary>Sets the no meme role for the server.</summary>
        SetNoMemeRole,
        /// <summary>Sets the no voice chat role for the server.</summary>
        SetNoVoiceChatRole,
        /// <summary>Overwrites the mute role with a new one for the server.</summary>
        OverwriteMuteRole,
        /// <summary>Overwrites the no meme role with a new one for the server.</summary>
        OverwriteNoMemeRole,
        /// <summary>Overwrites the no voice chat role with a new one for the server.</summary>
        OverwriteNoVoiceChatRole,
    }

    /// <summary>The Task allows the bot to store an action that's to be executed at a later time. Tempbans, tempmutes, reminders and other commands use these to make sure that even when the bot is offline, these actions will be executed once brought back online.</summary>
    /// <remarks>Used to execute actions that take time.</remarks>
    public struct Task {
        /// <summary>The type of task to be executed.</summary>
        TaskType TaskType;
        /// <summary>Which guild the task was set in.</summary>
        ulong GuildId;
        /// <summary>Which channel the task was set in.</summary>
        ulong ChannelId;
        /// <summary>Who set the task.</summary>
        ulong UserId;
        /// <summary>When the task was set.</summary>
        DateTime SetAt;
        /// <summary>When the task is to be executed.</summary>
        DateTime SetOff;
        /// <summary>For use of only when the <see cref="Tomoe.Database.Classes.TaskType">TaskType</see> is set to <c>TaskType.Reminder</c>.</summary> 
        string? Contents;
    }

    public enum TaskType {
        Reminder,
        TempBan,
        TempMute,
        TempNoMeme,
        TempNoVoiceChat
    }

    public interface IDatabase {
        #region LocalGuildCache

        // Strike system
        int GetUserStrikes(ulong GuildId, ulong UserId);
        void AddUserStrike(ulong GuildId, ulong UserId, string StrikeReason);
        void RemoveUserStrike(ulong GuildId, ulong UserId, string PardonReason);
        void SetUserStrikes(ulong GuildId, ulong UserId, int Strikes);

        // Static roles system
        int GetUserRoles(ulong GuildId, ulong UserId);
        void AddUserRole(ulong GuildId, ulong UserId, ulong Role);
        void RemoveUserRole(ulong GuildId, ulong UserId, ulong Role);
        void SetUserRoles(ulong GuildId, ulong UserId, IEnumerator<ulong> Roles);

        // Mute system
        bool IsUserMuted(ulong GuildId, ulong UserId);
        void IsUserMuted(ulong GuildId, ulong UserId, bool IsMuted);

        // NoMeme system
        bool IsUserNoMemed(ulong GuildId, ulong UserId);
        void IsUserNoMemed(ulong GuildId, ulong UserId, bool IsNoMemed);

        // NoVoiceChat system
        bool IsUserNoVoiceChat(ulong GuildId, ulong UserId);
        bool IsUserNoVoiceChat(ulong GuildId, ulong UserId, bool IsNoVoiceChat);

        #endregion LocalGuildCache

        #region LocalGuildSettings

        void InsertGuildId(ulong GuildId);

        bool IsAntiInvite(ulong GuildId);
        void IsAntiInvite(ulong GuildId, bool IsAntiInvite);

        bool IsAntiDuplicate(ulong GuildId);
        void IsAntiDuplicate(ulong GuildId, bool IsAntiDuplicate);

        IEnumerator<string> AllowedInvites(ulong GuildId);
        void AllowedInvites(ulong GuildId, IEnumerator<string> AllowedInvites);

        int MaxLines(ulong GuildId);
        void MaxLines(ulong GuildId, int LineCount);

        int MaxMentions(ulong GuildId);
        void MaxMentions(ulong GuildId, int MaxMentions);

        bool AutoDehoist(ulong GuildId);
        void AutoDehoist(ulong GuildId, bool AutoDehoist);

        bool AutoRaidMode(ulong GuildId);
        void AutoRaidMode(ulong GuildId, bool AutoRaidMode);

        IEnumerator<ulong> IgnoredChannels(ulong GuildId);
        void IgnoredChannels(ulong GuildId, IEnumerator<ulong> Channels);

        IEnumerator<ulong> AdministraitiveRoles(ulong GuildId);
        void AdministraitiveRoles(ulong GuildId, IEnumerator<ulong> Roles);

        Dictionary<Action, ulong> LoggingChannels(ulong GuildId);
        void LoggingChannels(ulong GuildId, Dictionary<Action, ulong> Channels);
        ulong GetLoggingChannel(ulong GuildId, Action Action);
        void SetLoggingChannel(ulong GuildId, Action Action, ulong ChannelId);

        ulong GetMuteRole(ulong GuildId);
        void SetMuteRole(ulong GuildId, ulong MuteRole);

        ulong GetNoMemeRole(ulong GuildId);
        void SetNoMemeRole(ulong GuildId, ulong NoMemeRole);

        ulong GetNoVoiceChatRole(ulong GuildId);
        void SetNoVoiceChatRole(ulong GuildId, ulong NoVoiceChatRole);

        bool AntiRaidActive(ulong GuildId);
        void AntiRaidActive(ulong GuildId, bool AntiRaidActive);

        bool AntiRaidSetOff(ulong GuildId);
        void AntiRaidSetOff(ulong GuildId, short SetOff);

        #endregion LocalGuildSettings

        #region Tasks
        IEnumerator<Task> GetAllTasks();
        IEnumerator<Task> GetAllTasks(ulong UserId);
        void AddTask(TaskType TaskType, ulong GuildId, ulong ChannelId, ulong UserId, DateTime SetAt, DateTime SetOff, string Contents);
        void RemoveTask(ulong UserId, DateTime SetAt, DateTime SetOff, string Contents);

        #endregion Tasks
    }

    public class Postgresql {
        [JsonProperty("database_name")]
        public string DatabaseName;

        [JsonProperty("host")]
        public string Host;

        [JsonProperty("password")]
        public string Password;

        [JsonProperty("port")]
        public int Port;

        [JsonProperty("ssl_mode")]
        public string SslMode;

        [JsonProperty("username")]
        public string Username;
    }

    public class Sqlite {
        [JsonProperty("database_path")]
        public string DatabasePath;
    }
}