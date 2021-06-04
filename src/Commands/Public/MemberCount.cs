namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Threading.Tasks;

    public class MemberCount : SlashCommandModule
    {
        [SlashCommand("member_count", "Sends the approximate member count.")]
        public static async Task Overload(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = $"Approximate member count: {Api.Public.MemberCount[context.Guild.Id].ToMetric()}",
        });
    }
}
