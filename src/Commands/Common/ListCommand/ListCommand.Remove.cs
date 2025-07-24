using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Removes an item from a list.
        /// </summary>
        [Command("remove")]
        public static async ValueTask RemoveAsync(CommandContext context, Ulid itemId)
        {
            ListItemModel? item = await ListItemModel.GetAsync(itemId);
            if (item is null)
            {
                await context.RespondAsync($"No item with ID `{itemId}` found.");
                return;
            }

            await item.DeleteAsync();
            await context.RespondAsync($"Item `{itemId}` ({(item.IsChecked ? "Checked" : "Unchecked")}) has been deleted:\n> {item.Content}");
        }
    }
}
