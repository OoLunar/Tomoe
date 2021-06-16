namespace Tomoe.Db
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class GuildConfig
    {
        public bool AntiInvite { get; internal set; } = true;
        public bool AutoDehoist { get; internal set; }
        public bool AutoDelete { get; internal set; }
        public bool ProgressiveStrikes { get; internal set; } = true;
        public bool PersistentRoles { get; internal set; } = true;
        public bool AutoStrike { get; internal set; }
        public int MaxLinesPerMessage { get; internal set; } = 5;
        public int MaxUniqueMentionsPerMessage { get; internal set; } = 5;
        public List<string> AllowedInvites { get; } = new();
        public List<string> Prefixes { get; } = new();
        public List<ulong> AdminRoles { get; } = new();
        public List<ulong> IgnoredChannels { get; } = new();
        public ulong AntimemeRole { get; internal set; }
        [Key]
        public ulong Id { get; internal set; }
        public ulong MuteRole { get; internal set; }
        public ulong VoicebanRole { get; internal set; }
        public bool ShowPermissionErrors { get; internal set; } = true;
    }
}