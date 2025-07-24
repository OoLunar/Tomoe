using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Deletes a list.
        /// </summary>
        [Command("delete")]
        public static async ValueTask DeleteAsync(CommandContext context, [RemainingText] string name)
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

            int itemCount = await ListItemModel.CountAsync(list.Id);
            await list.DeleteAsync();
            await context.RespondAsync($"Deleted the list `{name}` with ID `{list.Id}`. You had {itemCount} items in this list, which have also been deleted.");
        }
    }
}
