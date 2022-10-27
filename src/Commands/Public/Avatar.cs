using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("avatar", "Gets someone's profile picture, optionally with a preferred image format and dimensions.")]
        public static async Task Avatar(InteractionContext context, [Option("User", "Who's avatar to retrieve.")] DiscordUser user = null)
        {
            user ??= context.Member;
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
            {
                Title = user.Username + (user.Username.EndsWith('s') ? "' Avatar" : "'s Avatar"),
                ImageUrl = user.GetAvatarUrl(ImageFormat.Png),
                Color = new DiscordColor("#7b84d1")
            }));
        }
    }
}
