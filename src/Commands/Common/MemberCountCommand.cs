using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// 1... 2... 3... 4... I declare a thumb war!
    /// </summary>
    public static class MemberCountCommand
    {
        /// <summary>
        /// Sends the number of members in the guild.
        /// </summary>
        [Command("member_count"), RequireGuild, TextAlias("mc")]
        public static async ValueTask ExecuteAsync(CommandContext context) => await context.RespondAsync($"Current member count: {await GuildMemberModel.CountMembersAsync(context.Guild!.Id):N0}");
    }
}
