namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class RoleInfo : SlashCommandModule
    {
        [SlashCommand("role_info", "Gets general information about a role.")]
        public static async Task ByProgram(InteractionContext context, [Option("role", "The role to get information on.")] DiscordRole discordRole) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(Api.Public.RoleInfo(context, discordRole)));
    }
}
