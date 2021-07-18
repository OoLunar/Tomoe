namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Threading.Tasks;

    public partial class Public : SlashCommandModule
    {
        [SlashCommand("member_count", "Sends the approximate member count.")]
        public static async Task MemberCount(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = $"Approximate member count: {TotalMemberCount[context.Guild.Id].ToMetric()}",
        });
    }
}
