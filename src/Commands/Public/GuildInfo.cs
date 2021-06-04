namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class GuildInfo : SlashCommandModule
    {

        [SlashCommand("guild_info", "Gets general info about the server.")]
        public static async Task Overload(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Api.Public.GuildInfo(context)));
    }
}
