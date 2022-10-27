namespace Tomoe.Commands
{
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using Humanizer;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("member_count", "Sends the approximate member count.")]
        public static async Task MemberCount(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = $"Approximate member count: {TotalMemberCount[context.Guild.Id].ToMetric()}",
        });
    }
}
