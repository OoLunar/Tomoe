using System.ComponentModel.DataAnnotations;

namespace Tomoe.Models
{
    public class GuildConfigModel
    {
        [Key]
        public ulong GuildId { get; init; }
        public bool IsEnabled { get; internal set; }
        public bool RestoreRoles { get; internal set; }
    }
}
