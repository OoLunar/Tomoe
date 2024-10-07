using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class TagCommand
    {
        /// <summary>
        /// Updates the content of a tag.
        /// </summary>
        /// <param name="name">The name of the tag to update.</param>
        /// <param name="content">The new content that the text wall should contain.</param>
        [Command("update")]
        public static async ValueTask UpdateTagAsync(CommandContext context, string name, [RemainingText] string content)
        {
            if (!TryVerifyTagName(await context.GetCultureAsync(), ref name, out string? error))
            {
                await context.RespondAsync(error);
                return;
            }
            else if (!TryVerifyTagContent(ref content, out error))
            {
                await context.RespondAsync(error);
                return;
            }

            TagModel? tag = await TagModel.FindAsync(name, context.Guild!.Id);
            if (tag is null)
            {
                await context.RespondAsync(string.Format(await context.GetCultureAsync(), TAG_NOT_FOUND, Formatter.Sanitize(name)));
                return;
            }
            else if (!TryVerifyTagOwnership(context, await context.GetCultureAsync(), tag, out error))
            {
                await context.RespondAsync(error);
                return;
            }

            await TagModel.UpdateAsync(tag.Id, tag.Name, content, context.User.Id);
            await context.RespondAsync($"Tag ``{Formatter.Sanitize(name)}`` updated.\n-# Tag Id: `{tag.Id}`");
        }
    }
}
