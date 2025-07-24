using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Adds an item to a list.
        /// </summary>
        [Command("add")]
        public static async ValueTask AddAsync(CommandContext context, string name, [RemainingText] string content)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await context.RespondAsync("You must provide a name for the list.");
                return;
            }
            else if (string.IsNullOrWhiteSpace(content))
            {
                await context.RespondAsync("You must provide content for the item.");
                return;
            }

            ListModel? list = await ListModel.GetListAsync(name, context.User.Id);
            if (list is null)
            {
                await context.RespondAsync($"You do not have a list named `{name}`.");
                return;
            }

            ListItemModel item = await ListItemModel.CreateAsync(list.Id, content);
            await context.RespondAsync($"Item added to `{name}`\n-# Item ID: `{item.Id}`");
        }
    }
}
