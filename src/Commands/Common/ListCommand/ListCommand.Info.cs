using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public static partial class ListCommand
    {
        /// <summary>
        /// Provides information about an existing list.
        /// </summary>
        [Command("info")]
        public static async ValueTask InfoAsync(CommandContext context, [RemainingText] string name)
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

            await context.RespondAsync(new DiscordEmbedBuilder()
            {
                Title = list.Name,
                Footer = new() { Text = "Use `/list view` to see your list's items!" }
            }
                .AddField("Item Count", (await ListItemModel.CountAsync(list.Id)).ToString("N0", await context.GetCultureAsync()))
                .AddField("Created At", Formatter.Timestamp(list.Id.Time))
                .AddField("Id", $"`{list.Id}`")
            );
        }
    }
}
