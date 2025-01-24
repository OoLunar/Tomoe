using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

namespace OoLunar.Tomoe.Commands.Interactivity
{
    public static partial class InteractivityCommand
    {
        [Command("paginate")]
        public static async ValueTask PaginateAsync(CommandContext context, params string[] pages)
        {
            if (pages.Length < 2)
            {
                await context.RespondAsync("You need to provide at least two pages.");
                return;
            }

            List<Page> pageList = [];
            foreach (string page in pages)
            {
                pageList.Add(new Page(new DiscordMessageBuilder().WithContent(page)));
            }

            await context.PaginateAsync(pageList);
        }
    }
}
