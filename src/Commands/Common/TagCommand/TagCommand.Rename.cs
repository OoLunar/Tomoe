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
        /// Creates a new text wall for later retrieval.
        /// </summary>
        /// <param name="currentName">The current name of the tag to rename.</param>
        /// <param name="newName">The new name of the current tag.</param>
        [Command("rename")]
        public static async ValueTask RenameTagAsync(CommandContext context, string currentName, string newName)
        {
            if (!TryVerifyTagName(ref currentName, out string? error))
            {
                await context.RespondAsync(error);
                return;
            }
            else if (!TryVerifyTagName(ref newName, out error))
            {
                await context.RespondAsync(error);
                return;
            }

            TagModel? tag = await TagModel.FindAsync(currentName, context.Guild!.Id);
            if (tag is null)
            {
                await context.RespondAsync(string.Format(CultureInfo.InvariantCulture, TAG_NOT_FOUND, Formatter.Sanitize(currentName)));
                return;
            }
            else if (!TryVerifyTagOwnership(context, tag, out error))
            {
                await context.RespondAsync(error);
                return;
            }
            else if (await TagModel.ExistsAsync(newName, context.Guild!.Id))
            {
                await context.RespondAsync(string.Format(CultureInfo.InvariantCulture, TAG_EXISTS, Formatter.Sanitize(newName)));
                return;
            }

            await TagModel.UpdateAsync(tag.Id, newName, tag.Content, context.User.Id);
            await context.RespondAsync($"Renamed tag ``{Formatter.Sanitize(currentName)}`` to ``{Formatter.Sanitize(newName)}``.\n-# Tag Id: `{tag.Id}`");
        }
    }
}
