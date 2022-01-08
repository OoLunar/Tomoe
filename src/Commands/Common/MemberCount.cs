using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class MemberCount : BaseCommandModule
    {
        [Command("member_count"), Description("Sends the approximate member count."), Aliases("mc")]
        public async Task MemberCountAsync(CommandContext context) => await context.RespondAsync($"Member count: {context.Guild.MemberCount.ToMetric()}");
    }
}