using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class TagCommand
    {
        /// <summary>
        /// Sends general information about a tag.
        /// </summary>
        /// <param name="user">Fetches the tags owned by this user.</param>
        [Command("list")]
        public static async ValueTask ListTagsAsync(CommandContext context, DiscordUser? user = null)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"List of tags {(user is not null ? $"owned by {user.Username}" : "within this server.")}",
                Color = new DiscordColor(0x6b73db)
            };

            await foreach (TagModel tag in TagModel.GetTagsAsync(context.Guild!.Id, user?.Id ?? 0))
            {
                if (embedBuilder.Fields.Count >= 25)
                {
                    break;
                }

                embedBuilder.AddField(tag.Name, $"``{tag.Id}`` - Created {Formatter.Timestamp(tag.Id.Time)}\nUpdated {Formatter.Timestamp(tag.LastUpdatedAt)}\nUsed {tag.Uses:N0} time{(tag.Uses == 1 ? "" : "s")}.", true);
            }

            if (embedBuilder.Fields.Count == 0)
            {
                embedBuilder.Description = "No tags found.";
            }

            await context.RespondAsync(embedBuilder);
        }
    }
}
