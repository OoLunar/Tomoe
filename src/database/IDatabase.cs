namespace Tomoe.Database.Interfaces {
    public interface IDatabase {
        IUser User { get; }
        IGuild Guild { get; }
        //ITasks Tasks { get; }
    }

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

    public interface IGuild {
        bool AntiInvite(ulong guildId);
        void AntiInvite(ulong guildId, bool invitesEnabled);

        string[] AllowedInvites(ulong guildId);
        void AllowedInvites(ulong guildId, string[] invites);
        bool IsAllowedInvite(ulong guildId, string invite);
        void AddAllowedInvite(ulong guildId, string invite);
        void RemoveAllowedInvite(ulong guildId, string invite);

        int MaxLines(ulong guildId);
        void MaxLines(ulong guildId, int maxLineCount);

        int MaxMentions(ulong guildId);
        void MaxMentions(ulong guildId, int maxMentionCount);

        bool AutoDehoist(ulong guildId);
        void AutoDehoist(ulong guildId, bool autoDehoist);

        bool AutoRaidMode(ulong guildId);
        void AutoRaidMode(ulong guildId, bool raidModeEnabled);

        ulong[] IgnoredChannels(ulong guildId);
        void IgnoredChannels(ulong guildId, ulong[] channelIds);
        bool IsIgnoredChannel(ulong guildId, ulong channelId);
        void AddIgnoredChannel(ulong guildId, ulong channelId);
        void RemoveIgnoredChannel(ulong guildId, ulong channelId);

        ulong[] AdminRoles(ulong guildId);
        void AdminRoles(ulong guildId, ulong[] roleIds);
        bool IsAdminRole(ulong guildId, ulong roleId);
        void AddAdminRole(ulong guildId, ulong roleId);
        void RemoveAdminRole(ulong guildId, ulong roleId);

        //TODO: Create Logging channels.

        void InsertGuildId(ulong guildId);
        bool GuildIdExists(ulong guildId);

        ulong MuteRole(ulong guildId);
        void MuteRole(ulong guildId, ulong roleId);

        ulong NoMemeRole(ulong guildId);
        void NoMemeRole(ulong guildId, ulong roleId);

        ulong NoVCRole(ulong guildId);
        void NoVCRole(ulong guildId, ulong roleId);

        bool AntiRaid(ulong guildId);
        void AntiRaid(ulong guildId, bool isEnabled);

        int AntiRaidSetOff(ulong guildId);
        void AntiRaidSetOff(ulong guildId, int setOff);
    }

    // Better not be faking them.
    public interface ITasks {

    }
}