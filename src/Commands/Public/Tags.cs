
namespace Tomoe.Commands.Public
{
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    [SlashCommandGroup("tag", "Sends a predetermined message.")]
    public class Tags : SlashCommandModule
    {
        public Database Database { private get; set; }

        [SlashCommand("send", "Sends a pretermined message.")]
        public async Task Send(InteractionContext context, [Option("tag_name", "The name of the tag to send")] string tagName)
        {
            Tag tag = await GetTagAsync(tagName);
            if (tag == null)
            {
                await Program.SendMessage(context, $"Tag `{tagName.ToLowerInvariant()}` was not found!");
            }
            else
            {
                tag.Uses++;
                await Database.SaveChangesAsync();
                await context.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = tag.Content
                });
            }
        }

        [SlashCommand("create", "Creates a new tag.")]
        public async Task Create(InteractionContext context, [Option("tag_name", "What to call the new tag.")] string tagName, [Option("tag_content", "What to fill the new tag with.")] string tagContent)
        {
            if ((await GetTagAsync(tagName)) != null)
            {
                await Program.SendMessage(context, $"Tag `{tagName.ToLowerInvariant()}` already exists!");
                return;
            }

            Tag tag = new();
            tag.Name = tagName.ToLowerInvariant();
            tag.Content = tagContent;
            tag.GuildId = context.Guild.Id;
            tag.OwnerId = context.User.Id;
            tag.TagId = Database.Tags.Count(databaseTag => databaseTag.GuildId == context.Guild.Id) + 1;
            Database.Tags.Add(tag);
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, $"Created tag `{tag.Name}` with content:\n{tag.Content}");
        }

        public async Task<Tag> GetTagAsync(string tagName)
        {
            tagName = tagName.ToLowerInvariant();
            Tag tag = Database.Tags.FirstOrDefault(databaseTag => databaseTag.Name == tagName);

            // Test if the tag is an alias
            if (tag != null && tag.IsAlias)
            {
                // Retrieve the original tag
                tag = Database.Tags.FirstOrDefault(databaseTag => databaseTag.Name == tag.AliasTo);
                // This shouldn't happen since Tags.Delete should also delete aliases, but it's here for safety.
                if (tag == null)
                {
                    Database.Tags.Remove(tag);
                    await Database.SaveChangesAsync();
                    return null;
                }
            }
            return tag;
        }
    }
}