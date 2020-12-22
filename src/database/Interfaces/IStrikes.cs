using System;

namespace Tomoe.Database.Interfaces {
    public struct Strikes {
        public ulong GuildId;
        public ulong VictimId;
        public ulong IssuerId;
        public string[] Reason;
        public string JumpLink;
        public bool VictimMessaged;
        public bool Dropped;
        public DateTime CreatedAt;
        public int Id;
    }
    public interface IStrikes {
        Strikes Get(int strikeId);
        Strikes[] GetVictim(ulong guildId, ulong victimId);
        Strikes[] GetIssued(ulong guildId, ulong issuerId);
        int Add(ulong guildId, ulong victimId, ulong issuerId, string reason, string jumpLink, bool victimMessaged);
        void Drop(int strikeId);
        void Edit(int strikeId, string reason);
    }
}