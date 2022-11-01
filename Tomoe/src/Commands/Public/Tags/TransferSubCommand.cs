using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Tomoe.Models;

namespace Tomoe.Commands.Common
{
    public sealed partial class TagCommand : ApplicationCommandModule
    {
        [SlashCommand("transfer", "Transfer ownership of a tag to another person.")]
        public async Task TransferAsync(InteractionContext context, [Option("name", "Which tag to transfer")] string tagName, [Option("New_Tag_Owner", "Who to transfer the tag too.")] DiscordUser? newTagOwner = null)
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
            else if (!await CanModifyTagAsync(tag, context.User.Id, context.Guild))
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: You don't have permission to transfer tag `{tag.Name}`!",
                    IsEphemeral = true
                });
            }
            else
            {
                newTagOwner ??= context.User;
                tag.OwnerId = newTagOwner.Id;
                await Database.SaveChangesAsync();
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Tag {Formatter.InlineCode(tag.Name)} has successfully been transferred to {newTagOwner.Mention}!",
                    IsEphemeral = true
                });
            }
        }
    }
}
