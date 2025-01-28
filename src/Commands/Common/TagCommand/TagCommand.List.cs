using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

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
            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.WithContent($"List of tags {(user is not null ? $"owned by {user.Username}" : "within this server.")}");

            List<Page> pages = [];
            await foreach (TagModel tag in TagModel.GetTagsAsync(context.Guild!.Id, user?.Id ?? 0))
            {
                string[] tagContent = tag.Content.Length < 256 ? [tag.Content] : tag.Content.Split(['\n', '.']);
                DiscordEmbedBuilder embedBuilder = new()
                {
                    Title = tag.Name,
                    Description = tagContent.Length > 1 ? $"{tagContent[0]}…" : tag.Content,
                    Color = new DiscordColor(0x6b73db),
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Tag Id: {tag.Id}"
                    }
                };

                embedBuilder.AddField("Created", Formatter.Timestamp(tag.Id.Time), true);
                embedBuilder.AddField("Updated", Formatter.Timestamp(tag.LastUpdatedAt), true);
                embedBuilder.AddField("Uses", $"{tag.Uses:N0} time{(tag.Uses == 1 ? "" : "s")}", true);

                pages.Add(new Page(new DiscordMessageBuilder(messageBuilder).AddEmbed(embedBuilder), embedBuilder.Title));
            }

            if (pages.Count == 0)
            {
                await context.RespondAsync("No tags found.");
                return;
            }

            pages.Sort((a, b) => a.Title!.CompareTo(b.Title));
            await context.PaginateAsync(pages);
        }
    }
}
