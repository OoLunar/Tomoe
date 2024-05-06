using System;
using System.ComponentModel;

namespace OoLunar.Tomoe.Database.Models
{
    [Flags]
    public enum GuildMemberState
    {
        /// <summary>
        /// The member is present and has no special flags.
        /// </summary>
        [Description("Nothing of note")]
        None = 0,

        /// <summary>
        /// The member has previously joined the guild however is not currently present.
        /// </summary>
        [Description("Not in the guild")]
        Absent = 1 << 0,

        /// <summary>
        /// The member is currently muted.
        /// </summary>
        [Description("Currently muted")]
        Muted = 1 << 1,

        /// <summary>
        /// The member is currently banned.
        /// </summary>
        [Description("Currently banned")]
        Banned = 1 << 2,
    }
}
