using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Models;

namespace Tomoe.Commands.Common
{
    public sealed partial class TagCommand : ApplicationCommandModule
    {
        [SlashCommand("delete_aliases", "Removes all aliases from a tag.")]
        public async Task DeleteAliasesAsync(InteractionContext context, [Option("name", "Which tag to remove all aliases from.")] string tagName)
        {
            Tag? tag = await GetTagAsync(tagName, context.Guild.Id);
            if (tag is null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Tag `{tagName.ToLowerInvariant()}` does not exist!",
                    IsEphemeral = true
                });
            }
            else if (tag.IsAlias)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Tag `{tagName.ToLowerInvariant()}` is an alias! Use {Formatter.InlineCode($"/tag info {tag.Name}")} to get the original tag name!",
                    IsEphemeral = true
                });
            }
            else if (!await CanModifyTagAsync(tag, context.User.Id, context.Guild))
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: You don't have permission to remove aliases for tag `{tag.Name}`!",
                    IsEphemeral = true
                });
            }
            else
            {
                IEnumerable<Tag> tags = Database.Tags.Where(databaseTag => databaseTag.AliasTo == tag.Name);
                Database.Tags.RemoveRange(tags);
                await Database.SaveChangesAsync();
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"All aliases on tag {Formatter.InlineCode(tag.Name)} has been deleted!"
                });
            }
        }
    }
}
