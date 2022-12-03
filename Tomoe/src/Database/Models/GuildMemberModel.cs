using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class GuildMemberModel
    {
        public Guid Id { get; private set; }
        public ulong UserId { get; private set; }
        public ulong GuildId { get; private set; }
        public DateTime JoinedAt { get; private set; }

        public GuildMemberModel() { }
        public GuildMemberModel(DiscordMember member)
        {
            UserId = member.Id;
            GuildId = member.Guild.Id;
            JoinedAt = member.JoinedAt.UtcDateTime;
        }
    }
}
