using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Db;

namespace Tomoe.Commands.Common
{
    public sealed partial class TagCommand : ApplicationCommandModule
    {
        [SlashCommand("edit", "Edits a tag's text.")]
        public async Task EditAsync(InteractionContext context, [Option("name", "Which tag to edit.")] string tagName, [Option("Tag_Content", "What to fill the tag with.")] string tagContent)
        {
            Tag? tag = await GetTagAsync(tagName, context.Guild.Id);
            if (tag == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Tag `{tagName.ToLowerInvariant()}` does not exist!",
                    IsEphemeral = true
                });
            }
            else if (!await CanModifyTagAsync(tag, context.User.Id, context.Guild))
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
