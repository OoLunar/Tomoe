namespace Tomoe.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using Tomoe.Db;

    public partial class Public : ApplicationCommandModule
    {
        public partial class Tags : ApplicationCommandModule
        {
            [SlashCommand("delete", "Deletes a tag.")]
            public async Task Delete(InteractionContext context, [Option("name", "Which tag to remove permanently.")] string tagName)
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
                        Content = $"Error: You don't have permission to delete tag `{tag.Name}`!",
                        IsEphemeral = true
                    });
                }
                else
                {
                    IEnumerable<Tag> tags = Database.Tags.Where(databaseTag => databaseTag.AliasTo == tag.Name);
                    Database.Tags.RemoveRange(tags.Append(tag));
                    await Database.SaveChangesAsync();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Tag {Formatter.InlineCode(tag.Name)} successfully deleted!",
                    });
                }
            }
        }
    }
}
