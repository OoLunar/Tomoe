namespace Tomoe.Database.Interfaces {
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

        ulong? MuteRole(ulong guildId);
        void MuteRole(ulong guildId, ulong roleId);

        ulong? NoMemeRole(ulong guildId);
        void NoMemeRole(ulong guildId, ulong roleId);

        ulong? NoVCRole(ulong guildId);
        void NoVCRole(ulong guildId, ulong roleId);

        bool AntiRaid(ulong guildId);
        void AntiRaid(ulong guildId, bool isEnabled);

        int AntiRaidSetOff(ulong guildId);
        void AntiRaidSetOff(ulong guildId, int setOff);
    }
}