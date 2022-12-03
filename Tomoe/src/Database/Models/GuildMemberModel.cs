using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using EdgeDB;

namespace OoLunar.Tomoe.Database.Models
{
    [EdgeDBType("GuildMember")]
    public sealed class GuildMemberModel : IDatabaseModel<GuildMemberModel, ulong>
    {
        [EdgeDBProperty("user_id")]
        public ulong Id { get; private set; }
        public GuildModel Guild { get; private set; }
        public DateTimeOffset JoinedAt { get; private set; }

        public GuildMemberModel(DiscordMember member, GuildModel guild)
        {
            Guild = guild;
            Id = member.Id;
            JoinedAt = member.JoinedAt;
        }

        public static Task<GuildMemberModel?> CreateAsync(GuildMemberModel entity, EdgeDBClient client) => QueryBuilder.Insert<GuildMemberModel>(model => entity).UnlessConflictOn(model => model.Id).ExecuteAsync(client);
        public static Task<GuildMemberModel?> UpdateAsync(GuildMemberModel entity, EdgeDBClient client) => Task.FromResult(entity ?? null);
        public static async Task<GuildMemberModel?> FindAsync(ulong id, EdgeDBClient client) => (await QueryBuilder.Select<GuildMemberModel>().Filter(model => model.Id == id).ExecuteAsync(client, Capabilities.ReadOnly)).FirstOrDefault();
        public static async Task<GuildMemberModel?> DeleteAsync(ulong id, EdgeDBClient client) => (await QueryBuilder.Delete<GuildMemberModel>().Filter(model => model.Id == id).ExecuteAsync(client, Capabilities.Modifications)).FirstOrDefault();
    }
}
