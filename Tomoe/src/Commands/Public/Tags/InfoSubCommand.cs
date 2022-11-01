using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Db;

namespace Tomoe.Commands.Common
{
    public sealed partial class TagCommand : ApplicationCommandModule
    {
        [SlashCommand("info", "Sends general information on the requested tag.")]
        public async Task InfoAsync(InteractionContext context, [Option("name", "Which tag to gather information on.")] string tagName)
        {
            Tag? tag = await GetTagAsync(tagName, context.Guild.Id);
            if (tag == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"Error: Tag `{tagName.ToLowerInvariant()}` does not exist!",
                    IsEphemeral = true
                });
                return;
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#7b84d1"),
                Title = "Information on tag " + tag.Name
            };
            embedBuilder.AddField("Is An Alias", tag.IsAlias.ToString());

            if (tag.IsAlias)
            {
                embedBuilder.AddField("Alias To", tag.AliasTo);
            }
            else
            {
                embedBuilder.AddField("Aliases", string.Join(", ", Database.Tags.Where(databaseTag => databaseTag.AliasTo == tag.Name).Select(databaseTag => databaseTag.Name)));
                embedBuilder.Description = tag.Content;
            }

            embedBuilder.AddField("Created At", tag.CreatedAt.ToOrdinalWords());
            embedBuilder.AddField("Global Id", $"`{tag.Id}`");
            embedBuilder.AddField("Tag Name", tag.Name);
            embedBuilder.AddField("Owner", $"<@{tag.OwnerId}>");
            embedBuilder.AddField("Local Id", '#' + tag.TagId.ToString(CultureInfo.InvariantCulture));
            embedBuilder.AddField("Total Uses", tag.Uses.ToMetric());

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
        }
    }
}
