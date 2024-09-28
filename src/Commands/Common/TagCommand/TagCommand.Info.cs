using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class TagCommand
    {
        /// <summary>
        /// Sends general information about a tag.
        /// </summary>
        /// <param name="name">The name of the tag to retrieve information for.</param>
        [Command("info")]
        public static async ValueTask InfoTagAsync(CommandContext context, [RemainingText] string name)
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

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Info for tag {tag.Name}",
                Color = new DiscordColor("#6b73db")
            };

            embedBuilder.AddField("Tag Id", $"`{tag.Id}`", true);
            embedBuilder.AddField("Owner", $"<@{tag.OwnerId}>", true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(tag.Id.Time, TimestampFormat.LongDateTime), true);
            embedBuilder.AddField("Last Updated", Formatter.Timestamp(tag.LastUpdatedAt), true);
            embedBuilder.AddField("Total Edit Count", $"{await TagHistoryModel.CountRevisionsAsync(tag.Id):N0}", true);
            embedBuilder.AddField("Total Times Used", tag.Uses.ToString("N0", CultureInfo.InvariantCulture), true);
            await context.RespondAsync(embedBuilder);
        }
    }
}
