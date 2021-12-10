namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("invite", "Sends the link to add Tomoe to a guild.")]
        public static async Task Invite(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            // TODO: Calculate proper permissions.
            Content = Formatter.EmbedlessUrl(new($"https://discord.com/api/oauth2/authorize?client_id={context.Client.CurrentUser.Id}&scope=applications.commands%20bot&permissions=8")),
            IsEphemeral = true
        });
    }
}
