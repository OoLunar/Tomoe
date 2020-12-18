namespace Tomoe.Database.Interfaces {
    public interface IUser {
        void InsertUser(ulong guildId, ulong userId);

        ulong[] GetRoles(ulong guildId, ulong userId);
        void AddRole(ulong guildId, ulong userId, ulong roleId);
        void RemoveRole(ulong guildId, ulong userId, ulong roleId);
        void SetRoles(ulong guildId, ulong userId, ulong[] roleIds);

        int GetStrikeCount(ulong guildId, ulong userId);
        void AddStrike(ulong guildId, ulong userId);
        void RemoveStrike(ulong guildId, ulong userId);
        void SetStrikeCount(ulong guildId, ulong userId, int strikeCount);

        bool IsMuted(ulong guildId, ulong userId);
        void IsMuted(ulong guildId, ulong userId, bool isMuted);

        bool IsNoMemed(ulong guildId, ulong userId);
        void IsNoMemed(ulong guildId, ulong userId, bool isNoMemed);

        bool IsNoVC(ulong guildId, ulong userId);
        void IsNoVC(ulong guildId, ulong userId, bool isNoVC);
    }
}