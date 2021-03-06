namespace Tomoe.Db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class GuildUser
    {
        public GuildUser(ulong userId) => UserId = userId;

        [Key] public int Id { get; internal set; }
        public ulong UserId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public List<ulong> Roles { get; } = new();
        public bool IsMuted { get; internal set; }
        public bool IsAntimemed { get; internal set; }
        public bool IsVoicebanned { get; internal set; }
        public DateTime JoinedAt { get; internal set; }
    }
}