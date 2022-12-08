using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class PollModel
    {
        public Guid Id { get; init; }
        public string Question { get; init; }
        public List<string> Options { get; init; }

        [Column(TypeName = "json")]
        public Dictionary<ulong, int> Votes { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }

        public PollModel() { }
        public PollModel(Guid id, string question, IEnumerable<string> options, DateTimeOffset expiresAt)
        {
            Id = id;
            Question = question;
            Options = options.ToList();
            Votes = new();
            ExpiresAt = expiresAt;
        }
    }
}
