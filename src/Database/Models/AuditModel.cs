using System;
using System.Collections.Generic;
using System.Linq;
using ConcurrentCollections;
using DSharpPlus;
using DSharpPlus.Entities;
using EdgeDB;
using EdgeDB.DataTypes;
using OoLunar.Tomoe.Database.Converters;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// Records notable actions done by guild members.
    /// </summary>
    [EdgeDBType("Audit")]
    public sealed class AuditModel : DatabaseTrackable<AuditModel>
    {
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
        public DiscordMember Authorizer { get; set; } = null!;

        /// <summary>
        /// Who was affected by the action.
        /// </summary>
        [EdgeDBIgnore]
        public IReadOnlyList<ulong> AffectedUsers => _affectedUsers.ToArray();
        [EdgeDBTypeConverter(typeof(IEnumerableTypeConverter<ulong, long>))]
        private ConcurrentHashSet<ulong> _affectedUsers { get; set; } = new();

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
        [EdgeDBIgnore]
        public IReadOnlyList<string> Notes => _notes.ToArray();
        [EdgeDBTypeConverter(typeof(IEnumerableTypeConverter<string, string>))]
        private ConcurrentHashSet<string> _notes { get; set; } = new();

        /// <summary>
        /// The optional duration that the action is set to last for.
        /// </summary>
        [EdgeDBProperty("duration_length")]
        public Range<DateTimeOffset>? Duration { get; internal set; }

        public AuditModel() { }
        public AuditModel(GuildModel guildModel, DiscordMember authorizer)
        {
            ArgumentNullException.ThrowIfNull(guildModel, nameof(guildModel));
            ArgumentNullException.ThrowIfNull(authorizer, nameof(authorizer));

            Guild = guildModel;
            Authorizer = authorizer;
        }

        /// <summary>
        /// Sets the reason for the audit, providing a default reason if none is provided.
        /// </summary>
        /// <param name="reason">The reason that the action was executed, optionally provided by the user.</param>
        /// <returns>If the reason was null or whitespace, it'll provide a bolded "[No reason was provided]". Otherwise it'll return a trimmed reason.</returns>
        public string SetReason(string? reason) => Reason = string.IsNullOrWhiteSpace(reason)
            ? Formatter.Bold("[No reason was provided]")
            : reason.Trim();

        /// <summary>
        /// Adds a note to the audit informing the viewers of something notable about the action.
        /// </summary>
        /// <param name="note">What to add to the note list.</param>
        public void AddNote(string note) => _notes.Add(note.Trim());

        /// <summary>
        /// Adds a user to the affected users list.
        /// </summary>
        /// <param name="userId">Who to add.</param>
        public void AddAffectedUsers(params ulong[] userIds)
        {
            for (int i = 0; i < userIds.Length; i++)
            {
                _affectedUsers.Add(userIds[i]);
            }
        }
    }
}
