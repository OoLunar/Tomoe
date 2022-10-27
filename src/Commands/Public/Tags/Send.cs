namespace Tomoe.Commands
{
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using Tomoe.Db;

    public partial class Public : ApplicationCommandModule
    {
        public partial class Tags : ApplicationCommandModule
        {
            [SlashCommand("send", "Sends a pretermined message.")]
            public async Task Send(InteractionContext context, [Option("name", "The name of the tag to send")] string tagName)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: Tag `{tagName.ToLowerInvariant()}` was not found!",
                        IsEphemeral = true
                    });
                }
                else
                {
                    tag.Uses++;
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = tag.Content
                    });
                }
            }
        }
    }
}
