using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Moments.Pagination
{
    public sealed record Page
    {
        /// <summary>
        /// The message content to display in the <see cref="DiscordMessage"/>.
        /// </summary>
        public DiscordMessageBuilder Message { get; init; }

        /// <summary>
        /// Page metadata, used for the select menu option name.
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Page metadata, used for the select menu option description.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Page metadata, used for the select menu option emoji.
        /// </summary>
        public DiscordEmoji? Emoji { get; init; }

        /// <summary>
        /// Constructs a new <see cref="Page" /> for use in <see cref="ExtensionMethods.PaginateAsync(DSharpPlus.Commands.CommandContext, System.Collections.Generic.IEnumerable{Page}, IPaginationComponentCreator?, System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <param name="message">The message to display in the <see cref="DiscordMessage"/>.</param>
        /// <param name="title">The title to display in a select menu option.</param>
        /// <param name="description">The description to show in a select menu option.</param>
        /// <param name="emoji">The emoji to show in a select menu option.</param>
        public Page(DiscordMessageBuilder message, string? title = null, string? description = null, DiscordEmoji? emoji = null)
        {
            if (message.Components.Count > 3)
            {
                throw new ArgumentException("The message must have at least two component rows available for pagination controls. Please ensure three component rows at most are added.", nameof(message));
            }

            if (title?.Length > 100)
            {
                title = title[..99] + '…';
            }

            if (description?.Length > 100)
            {
                description = description[..99] + '…';
            }

            Message = message;
            Title = title;
            Description = description;
            Emoji = emoji;
        }

        public DiscordMessageBuilder CreateMessage(PaginationMoment data)
        {
            DiscordMessageBuilder message = new(Message);
            return message.AddComponents([
                data.ComponentCreator.CreateFirstPageButton(data.Id, data.CurrentPageIndex, data.Pages),
                data.ComponentCreator.CreatePreviousPageButton(data.Id, data.CurrentPageIndex, data.Pages),
                data.ComponentCreator.CreateStopButton(data.Id, data.CurrentPageIndex, data.Pages),
                data.ComponentCreator.CreateNextPageButton(data.Id, data.CurrentPageIndex, data.Pages),
                data.ComponentCreator.CreateLastPageButton(data.Id, data.CurrentPageIndex, data.Pages)
            ])
            .AddComponents(data.ComponentCreator.CreateDropdown(data.Id, data.CurrentPageIndex, data.Pages));
        }
    }
}
