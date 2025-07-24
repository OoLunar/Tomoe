using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Lists the user's created lists.
        /// </summary>
        [Command("list"), DefaultGroupCommand]
        public static async ValueTask ListAsync(CommandContext context)
        {
            List<Page> pages = [];
            StringBuilder stringBuilder = new();
            await foreach (ListModel list in ListModel.GetAllListsAsync(context.User.Id))
            {
                string line = $"{list.Name}: {await ListItemModel.CountAsync(list.Id):N0} items";
                if (line.Length + stringBuilder.Length > 2000)
                {
                    pages.Add(new Page(new()
                    {
                        Content = stringBuilder.ToString()
                    }));

                    stringBuilder.Clear();
                }

                stringBuilder.AppendLine(line);
            }

            if (stringBuilder.Length > 0)
            {
                pages.Add(new Page(new()
                {
                    Content = stringBuilder.ToString()
                }));
            }

            if (pages.Count == 0)
            {
                await context.RespondAsync("You have no lists.");
            }
            else
            {
                await context.PaginateAsync(pages);
            }
        }
    }
}
