namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Public : SlashCommandModule
    {
        public partial class Tags : SlashCommandModule
        {
            [SlashCommand("alias", "Points one tag to another.")]
            public async Task Alias(InteractionContext context, [Option("old_tag", "Which tag to point to.")] string oldTagName, [Option("new_tag", "What to call the new alias.")] string newTagName)
            {
                Tag newTag = await GetTagAsync(newTagName, context.Guild.Id);
                if (newTag != null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: Tag `{newTagName.ToLowerInvariant()}` already exists!",
                        IsEphemeral = true
                    });
                }
                Tag oldTag = await GetTagAsync(oldTagName, context.Guild.Id);
                if (oldTag == null)
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"Error: Tag `{oldTagName.ToLowerInvariant()}` does not exist!",
                        IsEphemeral = true
                    });
                }

                Tag alias = new();
                alias.AliasTo = oldTag.Name;
                alias.GuildId = context.Guild.Id;
                alias.IsAlias = true;
                alias.Name = newTagName.ToLowerInvariant();
                alias.OwnerId = context.User.Id;
                alias.TagId = Database.Tags.Count(databaseTag => databaseTag.GuildId == context.Guild.Id) + 1;
                Database.Tags.Add(alias);
                await Database.SaveChangesAsync();
            }
        }
    }
}