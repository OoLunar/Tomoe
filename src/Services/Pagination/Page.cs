using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Services.Pagination
{
    /// <summary>
    /// Used for paginating through a Discord embed.
    /// </summary>
    public sealed class Page
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
        /// A message builder to use for the page.
        /// </summary>
        public DiscordMessageBuilder MessageBuilder { get; init; }

        /// <summary>
        /// Constructs a new <see cref="Page" /> for use in a <see cref="PaginatorService"/>.
        /// </summary>
        /// <param name="builder">The <see cref="PageBuilder"/> to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="PageBuilder.MessageBuilder"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when both <see cref="PageBuilder.MessageBuilder.Content"/> and <see cref="PageBuilder.MessageBuilder.Embed"/> are null.</exception>
        public Page(PageBuilder builder)
        {
            builder.Verify();

            Title = builder.Title;
            Description = builder.Description;
            Emoji = builder.Emoji;
            MessageBuilder = builder.MessageBuilder;
        }
    }
}
