using System;
using System.Collections.Generic;
using EdgeDB;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Database
{
    public sealed class PollOptionModel : ICopyable<PollOptionModel>
    {
        public Guid? Id { get; private set; }
        public string Option { get; private init; } = null!;
        public PollModel Poll { get; private init; } = null!;

        [EdgeDBDeserializer]
        private PollOptionModel(IDictionary<string, object?> raw)
        {
            Id = (Guid?)raw["id"];
            Option = (string)raw["option"]!;
            Poll = (PollModel)raw["poll"]!;
        }

        public PollOptionModel(string option, PollModel poll)
        {
            Option = option;
            Poll = poll;
        }

        public PollOptionModel Copy(PollOptionModel old)
        {
            Id = old.Id;
            return this;
        }
    }
}
