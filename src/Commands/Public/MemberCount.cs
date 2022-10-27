using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Humanizer;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("member_count", "Sends the approximate member count.")]
        public static Task MemberCountAsync(InteractionContext context) => context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = $"Approximate member count: {TotalMemberCount[context.Guild.Id].ToMetric()}",
        });
    }
}
