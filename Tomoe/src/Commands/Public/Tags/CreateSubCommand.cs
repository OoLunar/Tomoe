using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Models;

namespace Tomoe.Commands.Common
{
    [SlashCommandGroup("tag", "Manages text walls for later use on common scenarios.")]
    public sealed partial class TagCommand : ApplicationCommandModule
    {
        [SlashCommand("create", "Creates a new tag.")]
        public async Task CreateAsync(InteractionContext context, [Option("name", "What to call the new tag.")] string tagName, [Option("tag_content", "What to fill the new tag with.")] string tagContent)
        {
            Tag? tag = await GetTagAsync(tagName, context.Guild.Id);
            if (tag != null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Tag `{tagName.ToLowerInvariant()}` already exists!",
                    IsEphemeral = true
                });
            }
            else
            {
                tag = new(Database.Tags.Count(databaseTag => databaseTag.GuildId == context.Guild.Id) + 1, tagName.Trim().ToLowerInvariant(), tagContent, null, context.User.Id, context.Guild.Id, 0);
                Database.Tags.Add(tag);
                await Database.SaveChangesAsync();
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Tag {Formatter.InlineCode(tag.Name)} created!\n{tag.Content}"
                });
            }
        }
    }
}
