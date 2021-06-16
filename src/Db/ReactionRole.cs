namespace Tomoe.Db
{
    using System.ComponentModel.DataAnnotations;

    public class ReactionRole
    {
        [Key]
        public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public ulong MessageId { get; internal set; }
        public ulong RoleId { get; internal set; }
        public string EmojiName { get; internal set; }
    }
}