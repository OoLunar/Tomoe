using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public class MenuRole
    {
        [Key]
        public int Id { get; init; }
        public string ButtonId { get; init; }
        public ulong GuildId { get; init; }
        public ulong RoleId { get; init; }

        public MenuRole(string buttonId, ulong guildId, ulong roleId)
        {
            ButtonId = buttonId;
            GuildId = guildId;
            RoleId = roleId;
        }
    }
}
