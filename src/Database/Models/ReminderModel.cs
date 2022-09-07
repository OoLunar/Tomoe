using System;
using DSharpPlus;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class ReminderModel : DatabaseTrackable<ReminderModel>
    {
        /// <summary>
        /// Who the reminder is for.
        /// </summary>
        public ulong OwnerId { get; init; }

        /// <summary>
        /// The link to the message that created the reminder.
        /// </summary>
        public Uri MessageLink { get; init; } = null!;

        /// <summary>
        /// When the reminder was created at.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// When the reminder expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; private set; }

        /// <summary>
        /// The content of the reminder.
        /// </summary>
        public string? Content { get; private set; }

        /// <summary>
        /// The guild that the reminder is set in, if any.
        /// </summary>
        public GuildModel? Guild { get; init; }

        public ReminderModel() { }
        public ReminderModel(ulong ownerId, string messageLink, DateTimeOffset expiresAt, string? content = null, GuildModel? guild = null)
        {
            ArgumentNullException.ThrowIfNull(messageLink);
            if (string.IsNullOrWhiteSpace(messageLink))
            {
                throw new ArgumentException("The message link cannot be null, empty of whitespace.", nameof(messageLink));
            }
            else if (!Uri.TryCreate(messageLink, UriKind.Absolute, out Uri? uri))
            {
                throw new ArgumentException("The message link is not a valid URI.", nameof(messageLink));
            }
            // Else for scope
            else
            {
                OwnerId = ownerId;
                MessageLink = uri;
                Content = content;
                ExpiresAt = expiresAt;
                Guild = guild;
            }
        }

        public void SetContent(string? content = null) => Content = string.IsNullOrWhiteSpace(content)
            ? Formatter.Bold("[No context provided.]")
            : content.Trim();

        public void ChangeExpireDate(DateTimeOffset expiresAt)
        {
            if (expiresAt < DateTimeOffset.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(expiresAt), "The new expire time must not be in the past.");
            }
            ExpiresAt = expiresAt;
        }
    }
}
