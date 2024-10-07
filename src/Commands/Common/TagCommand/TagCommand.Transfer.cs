using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class TagCommand
    {
        /// <summary>
        /// Changes the owner of a tag.
        /// </summary>
        /// <remarks>
        /// Note: The new owner does NOT need to be in the server!
        /// </remarks>
        /// <param name="name">The name of the tag to transfer.</param>
        /// <param name="user">The new owner of the tag.</param>
        [Command("transfer")]
        public static async ValueTask TransferTagAsync(CommandContext context, string name, DiscordUser user)
        {
            if (!TryVerifyTagName(await context.GetCultureAsync(), ref name, out string? error))
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

            await TagModel.UpdateAsync(tag.Id, tag.Name, tag.Content, user.Id);
            await context.RespondAsync($"Tag ``{Formatter.Sanitize(name)}`` has been transferred to <@{user.Id}>.\n-# Tag Id: `{tag.Id}`");
        }
    }
}
