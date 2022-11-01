using System.ComponentModel.DataAnnotations;
using DSharpPlus;

namespace Tomoe.Db
{
    public class Lock
    {
        [Key]
        public int Id { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong RoleId { get; init; }
        public bool HadPreviousOverwrite { get; init; }
        public Permissions Allowed { get; init; }
        public Permissions Denied { get; init; }

        public Lock() { }

        public Lock(ulong guildId, ulong channelId, ulong roleId, bool hadPreviousOverwrite, Permissions allowed, Permissions denied)
        {
            GuildId = guildId;
            ChannelId = channelId;
            RoleId = roleId;
            HadPreviousOverwrite = hadPreviousOverwrite;
            Allowed = allowed;
            Denied = denied;
        }
    }
}
