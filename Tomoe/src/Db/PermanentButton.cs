using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public enum ButtonType
    {
        MenuRole
    }

    public class PermanentButton
    {
        [Key]
        public int Id { get; set; }
        public string ButtonId { get; set; }
        public ButtonType ButtonType { get; set; }
        public ulong GuildId { get; set; }
    }
}
