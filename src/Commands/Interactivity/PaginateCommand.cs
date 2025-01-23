using System.Threading.Tasks;
using DSharpPlus.Commands;

namespace OoLunar.Tomoe.Commands.Interactivity
{
    public static partial class InteractivityCommand
    {
        [Command("paginate")]
        public static async ValueTask PaginateAsync(CommandContext context, string question, params string[] pages)
        {
            if (pages.Length < 2)
            {
                await context.RespondAsync("You need to provide at least two pages.");
                return;
            }
        }
    }
}
