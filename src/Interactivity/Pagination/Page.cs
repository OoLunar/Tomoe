using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Pagination
{
    public sealed record Page
    {
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
        /// The content to send in the <see cref="DiscordMessage"/>.
        /// </summary>
        public string? Content { get; init; }

        /// <summary>
        /// The embed to send in the <see cref="DiscordMessage"/>.
        /// </summary>
        public DiscordEmbed? Embed { get; init; }

        /// <summary>
        /// Constructs a new <see cref="Page" /> for use in a <see cref="PaginatorService"/>.
        /// </summary>
        /// <param name="title">The title to display in a select menu option.</param>
        /// <param name="description">The description to show in a select menu option.</param>
        /// <param name="content">The message content to display in the <see cref="DiscordMessage"/>.</param>
        /// <param name="embed">The embed to display in the <see cref="DiscordMessage"/>.</param>
        /// <param name="emoji">The emoji to show in a select menu option.</param>
        public Page(string? content = null, DiscordEmbed? embed = null, string? title = null, string? description = null, DiscordEmoji? emoji = null)
        {
            if (content == null && embed == null)
            {
                throw new ArgumentException("Either content or embed must be specified.");
            }

            if (title?.Length > 100)
            {
                title = title[..99] + '…';
            }

            if (description?.Length > 100)
            {
                description = description[..99] + '…';
            }

            if (content?.Length > 2000)
            {
                content = content[..1999] + '…';
            }

            Title = title;
            Description = description;
            Emoji = emoji;
            Content = content;
            Embed = embed;
        }
    }
}
