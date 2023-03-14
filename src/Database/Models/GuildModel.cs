using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// Represents a <see cref="DiscordGuild"/> in the database, storing only the relevant information needed.
    /// </summary>
    public sealed class GuildModel
    {
        /// <summary>
        /// The snowflake given to the guild by Discord.
        /// </summary>
        public ulong Id { get; init; }

        public GuildModel() { }
        public GuildModel(DiscordGuild guild) => Id = guild.Id;

        public static bool operator ==(GuildModel? left, GuildModel? right) => Equals(left, right);
        public static bool operator !=(GuildModel? left, GuildModel? right) => !Equals(left, right);
        public override bool Equals(object? obj) => obj is GuildModel model && Id == model.Id;
        public override int GetHashCode() => HashCode.Combine(Id);
    }
}
