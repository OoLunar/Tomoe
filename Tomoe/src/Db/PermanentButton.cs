using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public class PermanentButton
    {
        [Key]
        public int Id { get; init; }
        public string ButtonId { get; init; }
        public ButtonType ButtonType { get; init; }
        public ulong GuildId { get; init; }

        public PermanentButton() { }

        public PermanentButton(string buttonId, ButtonType buttonType, ulong guildId)
        {
            ButtonId = buttonId;
            ButtonType = buttonType;
            GuildId = guildId;
        }
    }
}
