using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Manages prewritten text walls for the server.
    /// </summary>
    [Command("tag"), RequireGuild]
    public sealed partial class TagCommand
    {
        private const string TAG_NAME_EMPTY = "The tag name cannot be empty.";
        private const string TAG_CONTENT_EMPTY = "The tag content cannot be empty.";
        private static readonly CompositeFormat TAG_EXISTS = CompositeFormat.Parse("The tag ``{0}`` already exists.");
        private static readonly CompositeFormat TAG_NOT_FOUND = CompositeFormat.Parse("The tag ``{0}`` was not found.");
        private static readonly CompositeFormat TAG_MODIFY_PERMISSIONS = CompositeFormat.Parse("The tag ``{0}`` can not be modified because it is owned by <@{1}> and your missing the `Manage Messages` permission.");

        /// <summary>
        /// Sends a recorded message to the channel, presumably for someone else's benefit.
        /// </summary>
        /// <param name="name">The name of the tag to retrieve.</param>
        [Command("get"), DefaultGroupCommand]
        public static async ValueTask GetTagAsync(CommandContext context, [RemainingText] string name)
        {
            if (!TryVerifyTagName(ref name, out string? errorMessage))
            {
                await context.RespondAsync(errorMessage);
                return;
            }

            string? tag = await TagModel.GetContentAsync(name, context.Guild!.Id);
            await context.RespondAsync(tag ?? string.Format(CultureInfo.InvariantCulture, TAG_NOT_FOUND, Formatter.Sanitize(name)));
        }

        private static bool TryVerifyTagName(ref string name, [NotNullWhen(false)] out string? errorMessage)
        {
            name = name.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = TAG_NAME_EMPTY;
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static bool TryVerifyTagContent(ref string content, [NotNullWhen(false)] out string? errorMessage)
        {
            content = content.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                errorMessage = TAG_CONTENT_EMPTY;
                return false;
            }

            errorMessage = null;
            return true;
        }

        private static bool TryVerifyTagOwnership(CommandContext context, TagModel tag, [NotNullWhen(false)] out string? errorMessage)
        {
            if (tag.OwnerId != context.User.Id && !context.Member!.Permissions.HasPermission(DiscordPermissions.ManageMessages))
            {
                errorMessage = string.Format(CultureInfo.InvariantCulture, TAG_MODIFY_PERMISSIONS, Formatter.Sanitize(tag.Name), tag.OwnerId);
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
