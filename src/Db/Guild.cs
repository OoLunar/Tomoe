namespace Tomoe.Db
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class GuildConfig
    {
        public GuildConfig(ulong id) => Id = id;
        public bool AntiInvite { get; internal set; } = true;
        public bool AutoDehoist { get; internal set; }
        public bool AutoDelete { get; internal set; }
        public bool ProgressiveStrikes { get; internal set; } = true;
        public bool AutoStrike { get; internal set; }
        public int MaxLinesPerMessage { get; internal set; } = 5;
        public int MaxUniqueMentionsPerMessage { get; internal set; } = 5;
        public List<string> AllowedInvites { get; internal set; } = new();
        public List<string> Prefixes { get; internal set; } = new();
        public List<ulong> AdminRoles { get; internal set; } = new();
        public List<ulong> IgnoredChannels { get; internal set; } = new();
        public ulong AntimemeRole { get; internal set; }
        [Key] public ulong Id { get; }
        public ulong MuteRole { get; internal set; }
        public ulong VoicebanRole { get; internal set; }
        public bool ShowPermissionErrors { get; internal set; } = true;
    }
}