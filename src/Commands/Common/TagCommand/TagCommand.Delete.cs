using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class TagCommand
    {
        /// <summary>
        /// Deletes a tag and it's history.
        /// </summary>
        /// <param name="name">The name of the tag to delete.</param>
        [Command("delete")]
        public static async ValueTask DeleteTagAsync(CommandContext context, string name)
        {
            if (!TryVerifyTagName(ref name, out string? error))
            {
                await context.RespondAsync(error);
                return;
            }

            TagModel? tag = await TagModel.FindAsync(name, context.Guild!.Id);
            if (tag is null)
            {
                await context.RespondAsync(string.Format(CultureInfo.InvariantCulture, TAG_NOT_FOUND, Formatter.Sanitize(name)));
                return;
            }
            else if (!TryVerifyTagOwnership(context, tag, out error))
            {
                await context.RespondAsync(error);
                return;
            }

            await TagModel.DeleteAsync(tag.Id);
            await context.RespondAsync($"Tag ``{Formatter.Sanitize(name)}`` has been deleted.\n-# Tag Id: `{tag.Id}`");
        }
    }
}
