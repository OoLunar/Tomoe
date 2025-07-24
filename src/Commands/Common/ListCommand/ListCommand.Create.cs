using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Creates a new list.
        /// </summary>
        [Command("create")]
        public static async ValueTask CreateAsync(CommandContext context, [RemainingText] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await context.RespondAsync("You must provide a name for the list.");
                return;
            }
            else if (await ListModel.GetListAsync(name, context.User.Id) is not null)
            {
                await context.RespondAsync($"You already have a list named `{name}`.");
                return;
            }

            ListModel list = await ListModel.CreateListAsync(context.User.Id, name);
            await context.RespondAsync($"Created the list `{name}` with ID `{list.Id}`. You can now add items to it using the `list add` command.");
        }
    }
}
