using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Sends the entire list.
        /// </summary>
        [Command("view")]
        public static async ValueTask ViewAsync(CommandContext context, [RemainingText] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await context.RespondAsync("You must provide a name for the list.");
                return;
            }

            ListModel? list = await ListModel.GetListAsync(name, context.User.Id);
            if (list is null)
            {
                await context.RespondAsync($"You do not have a list named `{name}`.");
                return;
            }

            List<Page> pages = [];
            StringBuilder stringBuilder = new();
            int totalItemCount = await ListItemModel.CountAsync(list.Id);
            int currentItemStart = 1;
            int currentItem = 1;
            await foreach (ListItemModel item in ListItemModel.GetAllAsync(list.Id))
            {
                string line = $"- {(item.IsChecked ? ":white_check_mark:" : ":x:")} Added {Formatter.Timestamp(item.Id.Time)}: {item.Content}";
                if (line.Length + stringBuilder.Length > 2000)
                {
                    DiscordMessageBuilder messageBuilder = new();
                    messageBuilder.AddEmbed(new DiscordEmbedBuilder()
                    {
                        Description = stringBuilder.ToString(),
                        Footer = new()
                        {
                            Text = $"Currently viewing items {currentItemStart:N0}-{currentItem:N0} / {totalItemCount:N0}"
                        }
                    });

                    currentItemStart = currentItem;
                    pages.Add(new Page(messageBuilder));
                }

                currentItem++;
                stringBuilder.AppendLine(line);
            }

            if (stringBuilder.Length != 0)
            {
                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.AddEmbed(new DiscordEmbedBuilder()
                {
                    Description = stringBuilder.ToString(),
                    Footer = new()
                    {
                        Text = $"Currently viewing items {currentItemStart:N0}-{currentItem:N0} / {totalItemCount:N0}"
                    }
                });

                pages.Add(new Page(messageBuilder));
            }

            await context.PaginateAsync(pages);
        }
    }
}
