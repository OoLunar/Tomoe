using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public class AutoReaction
    {
        [Key]
        public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public string EmojiName { get; internal set; }
    }
}
