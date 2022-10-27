using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Db;

namespace Tomoe.Commands
{
    public partial class Public : ApplicationCommandModule
    {
        [SlashCommandGroup("tag", "Manages text walls for later use on common scenarios.")]
        public partial class Tags : ApplicationCommandModule
        {
            [SlashCommand("create", "Creates a new tag.")]
            public async Task Create(InteractionContext context, [Option("name", "What to call the new tag.")] string tagName, [Option("tag_content", "What to fill the new tag with.")] string tagContent)
            {
                Tag tag = await GetTagAsync(tagName, context.Guild.Id);
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
                    tag = new()
                    {
                        Name = tagName.ToLowerInvariant(),
                        Content = tagContent,
                        GuildId = context.Guild.Id,
                        OwnerId = context.User.Id,
                        TagId = Database.Tags.Count(databaseTag => databaseTag.GuildId == context.Guild.Id) + 1
                    };
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
}
