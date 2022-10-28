using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public class MenuRole
    {
        [Key]
        public int Id { get; set; }
        public string ButtonId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong RoleId { get; internal set; }
    }
}
