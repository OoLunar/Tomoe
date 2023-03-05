using System;
using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Services.Pagination
{
    /// <summary>
    /// A builder for <see cref="Page"/>.
    /// </summary>
    public sealed class PageBuilder
    {
        /// <inheritdoc cref="Page.Title"/>
        public string? Title { get; set; }

        /// <inheritdoc cref="Page.Description"/>
        public string? Description { get; set; }

        /// <inheritdoc cref="Page.Emoji"/>
        public DiscordEmoji? Emoji { get; set; }

        /// <inheritdoc cref="Page.MessageBuilder"/>
        public DiscordMessageBuilder MessageBuilder { get; set; } = new();

        /// <summary>
        /// Ensures that all data on the page is valid and can be used.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="MessageBuilder"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when both <see cref="MessageBuilder.Content"/> and <see cref="MessageBuilder.Embed"/> are null.</exception>
        [MemberNotNullWhen(true, nameof(MessageBuilder))]
        public void Verify()
        {
            if (MessageBuilder is null)
            {
                throw new ArgumentNullException(nameof(MessageBuilder));
            }
            else if (MessageBuilder.Content is null && MessageBuilder.Embed is null)
            {
                throw new ArgumentException("Either content or embed must be specified.");
            }

            Title?.Truncate(100, "…");
            Description?.Truncate(100, "…");
            MessageBuilder.Content?.Truncate(2000, "…");
        }

        public static implicit operator Page(PageBuilder builder) => new(builder);
    }
}
