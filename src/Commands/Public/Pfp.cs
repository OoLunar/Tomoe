namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class ProfilePicture : SlashCommandModule
    {
        [SlashCommand("profile_picture", "Gets someone's profile picture, optionally with a preferred image format and dimensions.")]
        public async Task Overload(InteractionContext context, [Option("User", "Who's avatar to retrieve.")] DiscordUser user = null)
        {
            user ??= context.Member;
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Api.Public.GetProfilePicture(user)));
        }
    }
}
