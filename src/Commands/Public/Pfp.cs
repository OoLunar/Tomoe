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
            await Program.SendMessage(context, null, new DiscordEmbedBuilder()
            {
                Title = user.Username + (user.Username.EndsWith('s') ? "' Avatar" : "'s Avatar"),
                ImageUrl = user.GetAvatarUrl(ImageFormat.Png, 1024),
                Color = new DiscordColor("#7b84d1")
            });
        }
    }
}
