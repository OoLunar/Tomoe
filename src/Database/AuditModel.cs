using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using EdgeDB;
using EdgeDB.DataTypes;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Database
{
    /// <summary>
    /// Records notable actions done by guild members.
    /// </summary>
    [EdgeDBType("Audit")]
    public sealed class AuditModel : ICopyable<AuditModel>
    {
        /// <summary>
        /// The database assigned id of the audit entry.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// When the audit log was created at.
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The guild that this audit entry is for.
        /// </summary>
        public GuildModel Guild { get; set; } = null!;

        /// <summary>
        /// Which user performed the action.
        /// </summary>
        public DiscordMember Authorizer { get; set; }

        /// <summary>
        /// Who was affected by the action.
        /// </summary>
        public IEnumerable<ulong>? AffectedUsers { get; set; }

        /// <summary>
        /// The reason for this action, provided by the user.
        /// </summary>
        public string Reason { get; set; } = Formatter.Bold("[No reason was provided]");

        /// <summary>
        /// Whether the action was successful or not.
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// Anything notable about the action, defined by the bot. May contain more information on an action, such as failure to DM a user or why the action wasn't successful.
        /// </summary>
        public IReadOnlyList<string>? Notes => _notes?.AsReadOnly();
        private List<string>? _notes { get; set; }

        /// <summary>
        /// The optional duration that the action is set to last for.
        /// </summary>
        [EdgeDBProperty("duration_length")]
        public Range<DateTimeOffset>? Duration { get; internal set; }

        public AuditModel(GuildModel guildModel, DiscordMember authorizer)
        {
            ArgumentNullException.ThrowIfNull(guildModel, nameof(guildModel));
            ArgumentNullException.ThrowIfNull(authorizer, nameof(authorizer));

            Guild = guildModel;
            Authorizer = authorizer;
        }

        /// <inheritdoc/>
        public AuditModel Copy(AuditModel old)
        {
            Id = old.Id;
            CreatedAt = old.CreatedAt;
            Guild = old.Guild;
            Authorizer = old.Authorizer;
            AffectedUsers = old.AffectedUsers;
            Reason = old.Reason;
            Successful = old.Successful;
            _notes = old._notes;
            Duration = old.Duration;

            return this;
        }

        /// <summary>
        /// Sets the reason for the audit, providing a default reason if none is provided.
        /// </summary>
        /// <param name="reason">The reason that the action was executed, optionally provided by the user.</param>
        /// <returns>If the reason was null or whitespace, it'll provide a bolded "[No reason was provided]". Otherwise it'll return a trimmed reason.</returns>
        public string SetReason(string? reason) => Reason = string.IsNullOrWhiteSpace(reason)
            ? Formatter.Bold("[No reason was provided]")
            : reason.Trim();

        public void AddNote(string note)
        {
            _notes ??= new List<string>();
            _notes.Add(note.Trim());
        }
    }
}
