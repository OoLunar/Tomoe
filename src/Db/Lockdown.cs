namespace Tomoe.Db
{
    using System.ComponentModel.DataAnnotations;
    using DSharpPlus;

    public class Lock
    {
        [Key]
        public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public ulong RoleId { get; internal set; }
        public bool HadPreviousOverwrite { get; internal set; }
        public Permissions Allowed { get; internal set; }
        public Permissions Denied { get; internal set; }
    }
}
