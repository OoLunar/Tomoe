using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("invite", "Sends the link to add Tomoe to a guild.")]
        public static Task InviteAsync(InteractionContext context) => context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            // TODO: Calculate proper permissions.
            Content = Formatter.EmbedlessUrl(new($"https://discord.com/api/oauth2/authorize?client_id={context.Client.CurrentUser.Id}&scope=applications.commands%20bot&permissions=8")),
            IsEphemeral = true
        });
    }
}
