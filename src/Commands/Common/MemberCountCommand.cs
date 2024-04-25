using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class MemberCountCommand
    {
        [Command("member_count"), RequireGuild]
        public static async ValueTask ExecuteAsync(CommandContext context) => await context.RespondAsync($"Current member count: {await GuildMemberModel.CountMembersAsync(context.Guild!.Id):N0}");
    }
}
