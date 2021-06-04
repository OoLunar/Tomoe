namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class GuildIcon : SlashCommandModule
    {
        public override Task BeforeExecutionAsync(InteractionContext context)
        {
            if (context.Guild == null)
            {
                context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "Error: This command can only be used in a guild!",
                    IsEphemeral = true
                });
            }

            return Task.CompletedTask;
        }

        [SlashCommand("guild_icon", "Gets the guild's icon.")]
        public static async Task Overload(InteractionContext context)
        {
            DiscordEmbed discordEmbed = Api.Public.GuildIcon(context);
            if (discordEmbed == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = "Error: The guild has no icon!"
                });
            }
            else
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed));
            }
        }
    }
}
