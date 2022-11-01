using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Humanizer;

namespace Tomoe.Commands.Common
{
    public sealed class MemberCountCommand : ApplicationCommandModule
    {
        [SlashCommand("member_count", "Sends the approximate member count.")]
        public static Task MemberCountAsync(InteractionContext context) => context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = $"Approximate member count: {Program.TotalMemberCount[context.Guild.Id].ToMetric()}",
        });
    }
}
