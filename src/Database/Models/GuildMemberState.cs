using System;

namespace OoLunar.Tomoe.Database.Models
{
    [Flags]
    public enum GuildMemberState : byte
    {
        /// <summary>
        /// The member is present and has no special flags.
        /// </summary>
        None,

        /// <summary>
        /// The member has previously joined the guild however is not currently present.
        /// </summary>
        Absent = 1 << 1,

        /// <summary>
        /// The member is currently muted.
        /// </summary>
        Muted = 1 << 2,

        /// <summary>
        /// The member is currently banned.
        /// </summary>
        Banned = 1 << 3,
    }
}
