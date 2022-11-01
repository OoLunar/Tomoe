using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Db;

namespace Tomoe.Commands.Common
{
    public sealed partial class TagCommand : ApplicationCommandModule
    {
        [SlashCommand("alias", "Points one tag to another.")]
        public async Task AliasAsync(InteractionContext context, [Option("old_tag", "Which tag to point to.")] string oldTagName, [Option("new_tag", "What to call the new alias.")] string newTagName)
        {
            Tag? newTag = await GetTagAsync(newTagName, context.Guild.Id);
            if (newTag != null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Tag `{newTagName.ToLowerInvariant()}` already exists!",
                    IsEphemeral = true
                });
                return;
            }

            Tag? oldTag = await GetTagAsync(oldTagName, context.Guild.Id);
            if (oldTag == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Tag `{oldTagName.ToLowerInvariant()}` does not exist!",
                    IsEphemeral = true
                });
                return;
            }

            Tag alias = new(Database.Tags.Count(databaseTag => databaseTag.GuildId == context.Guild.Id) + 1, newTagName.Trim().ToLowerInvariant(), null, oldTag.Name, context.User.Id, context.Guild.Id, 0);
            Database.Tags.Add(alias);
            await Database.SaveChangesAsync();
        }
    }
}
