using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Unchecks an item from a list.
        /// </summary>
        [Command("uncheck")]
        public static async ValueTask UncheckAsync(CommandContext context, Ulid itemId)
        {
            ListItemModel? item = await ListItemModel.GetAsync(itemId);
            if (item is null)
            {
                await context.RespondAsync($"No item with ID `{itemId}` found.");
                return;
            }

            item.IsChecked = false;
            await item.UpdateAsync();
            await context.RespondAsync($"Item `{itemId}` has been unchecked from the list:\n> {item.Content}");
        }
    }
}
