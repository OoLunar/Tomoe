using System.Threading.Tasks;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class MemberCountCommand
    {
        [Command("member_count"), RequireGuild]
        public static async Task ExecuteAsync(CommandContext context) => await context.RespondAsync($"Current member count: {context.Guild!.MemberCount:N0}");
    }
}
