namespace Tomoe.Db
{
    using System.ComponentModel.DataAnnotations;

    public class MenuRole
    {
        [Key]
        public int Id { get; set; }
        public string ButtonId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong RoleId { get; internal set; }
    }
}