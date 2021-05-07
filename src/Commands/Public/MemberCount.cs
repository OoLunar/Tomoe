namespace Tomoe.Commands.Public
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Humanizer;
    using System.Threading.Tasks;

    public class MemberCount : BaseCommandModule
    {
        [Command("member_count"), Description("Sends the approximate member count."), Aliases("mc")]
        public async Task Overload(CommandContext context) => await Program.SendMessage(context, $"Approximate member count: {context.Guild.MemberCount.ToMetric()}");
    }
}
