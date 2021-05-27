namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class Invite : SlashCommandModule
    {
        [SlashCommand("invite", "Sends the link to add Tomoe to a guild.")]
        public async Task Overload(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            IsEphemeral = true,
            Content = $"https://discord.com/api/oauth2/authorize?client_id={context.Client.CurrentUser.Id}&scope=applications.commands%20bot&permissions=8"
        });

    }
}
