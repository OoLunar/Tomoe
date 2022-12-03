using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class GuildModel
    {
        public ulong Id { get; private set; }

        public GuildModel() { }
        public GuildModel(DiscordGuild guild) => Id = guild.Id;
    }
}
