namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Public : ApplicationCommandModule
    {
        public partial class Tags : ApplicationCommandModule
        {
            [SlashCommand("edit", "Edits a tag's text.")]
            public async Task Edit(InteractionContext context, [Option("name", "Which tag to edit.")] string tagName, [Option("Tag_Content", "What to fill the tag with.")] string tagContent)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
                if (tag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: Tag `{tagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                }
                else if (!await CanModifyTag(tag, context.User.Id, context.Guild))
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: You don't have permission to edit tag `{tag.Name}`!",
                        IsEphemeral = true
                    });
                }
                else
                {
                    tag.Content = tagContent;
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag {Formatter.InlineCode(tag.Name)} successfully edited!\n{tag.Content}",
                        IsEphemeral = true
                    });

                }
            }
        }
    }
}

