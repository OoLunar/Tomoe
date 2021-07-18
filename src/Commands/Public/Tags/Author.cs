namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Public : SlashCommandModule
    {
        public partial class Tags : SlashCommandModule
        {
            [SlashCommand("author", "Gets the author of a tag.")]
            public async Task Author(InteractionContext context, [Option("name", "Which tag to gather information on.")] string tagName)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = tag == null ? $"Error: Tag `{tagName.ToLowerInvariant()}` does not exist!" : $"<@{tag.OwnerId}> ({tag.OwnerId})",
                    IsEphemeral = tag == null
                });
            }
        }
    }
}