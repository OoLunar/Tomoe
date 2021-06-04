namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class Invite : SlashCommandModule
    {
        [SlashCommand("invite", "Sends the link to add Tomoe to a guild.")]
        public static async Task Overload(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            IsEphemeral = true,
            Content = Api.Public.GetInvite(context.Client.CurrentUser.Id)
        });

    }
}
