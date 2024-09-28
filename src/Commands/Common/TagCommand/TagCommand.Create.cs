using System;
using System.Globalization;
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
        /// Creates a new text wall for later retrieval.
        /// </summary>
        /// <param name="name">The name of the tag to create.</param>
        /// <param name="content">The content that the text wall should contain.</param>
        [Command("create")]
        public static async ValueTask CreateTagAsync(CommandContext context, string name, [RemainingText] string content)
        {
            if (!TryVerifyTagName(ref name, out string? error))
            {
                await context.RespondAsync(error);
                return;
            }
            else if (!TryVerifyTagContent(ref content, out error))
            {
                await context.RespondAsync(error);
                return;
            }
            else if (await TagModel.ExistsAsync(name, context.Guild!.Id))
            {
                await context.RespondAsync(string.Format(CultureInfo.InvariantCulture, TAG_EXISTS, Formatter.Sanitize(name)));
                return;
            }

            Ulid tagId = Ulid.NewUlid();
            await TagModel.CreateAsync(tagId, name, content, context.User.Id, context.Guild!.Id);
            await context.RespondAsync($"Tag ``{Formatter.Sanitize(name)}`` created.\n-# Tag Id: `{tagId}`");
        }
    }
}
