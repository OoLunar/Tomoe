using System;
using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Services.Pagination
{
    public sealed class PageBuilder
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DiscordEmoji? Emoji { get; set; }
        public DiscordMessageBuilder MessageBuilder { get; set; } = new();

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
