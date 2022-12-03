using System.Threading.Tasks;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class MemberCountCommand : BaseCommand
    {
        [Command("member_count")]
        public static async Task ExecuteAsync(CommandContext context)
        {
            if (context.Guild is null)
            {
                await context.ReplyAsync($"Command `/{context.CurrentCommand.FullName}` can only be used in a guild.");
                return;
            }

            await context.ReplyAsync($"Current member count: {(await context.Guild.GetAllMembersAsync()).Count:N0}");
        }
    }
}
