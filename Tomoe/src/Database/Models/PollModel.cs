using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class PollModel
    {
        public Guid Id { get; init; }
        public string Question { get; init; }

        [Column(TypeName = "jsonb")]
        public Dictionary<string, string?> Options { get; init; }

        [Column(TypeName = "json")]
        public Dictionary<ulong, int> Votes { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }

        public PollModel() { }
        public PollModel(Guid id, string question, Dictionary<string, string?> options, DateTimeOffset expiresAt)
        {
            Id = id;
            Question = question;
            Options = options;
            Votes = new();
            ExpiresAt = expiresAt;
        }
    }
}
