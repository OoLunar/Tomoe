using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using EdgeDB;
using EdgeDB.Interfaces;

namespace OoLunar.Tomoe.Database.Models
{
    [EdgeDBType("Guild")]
    public sealed class GuildModel : IDatabaseModel<GuildModel, ulong>
    {
        [EdgeDBProperty("guild_id")]
        public ulong Id { get; private set; }
        public IReadOnlyList<GuildMemberModel> Members => _members.AsReadOnly();
        private List<GuildMemberModel> _members = new();

        public GuildModel(DiscordGuild guild)
        {
            Id = guild.Id;
            foreach (DiscordMember member in guild.Members.Values)
            {
                _members.Add(new GuildMemberModel(member, this));
            }
        }

        public static async Task<GuildModel?> CreateAsync(GuildModel entity, EdgeDBClient client) => (await ((IMultiCardinalityExecutable<GuildModel>)QueryBuilder
            .Insert<GuildModel>(model => entity)
            .UnlessConflictOn(model => model.Id)
            .Else(QueryBuilder.Update<GuildModel>(model => CombineMembers(model, entity.Members))))
            .ExecuteAsync(client, Capabilities.Modifications)).FirstOrDefault();

        public static async Task<GuildModel?> UpdateAsync(GuildModel entity, EdgeDBClient client) => (await QueryBuilder
            .Update<GuildModel>(model => entity)
            .Filter(model => model.Id == entity.Id)
            .ExecuteAsync(client, Capabilities.Modifications)).FirstOrDefault();

        public static async Task<GuildModel?> FindAsync(ulong id, EdgeDBClient client) => (await QueryBuilder.Select<GuildModel>().Filter(model => model.Id == id).ExecuteAsync(client, Capabilities.ReadOnly)).FirstOrDefault();
        public static async Task<GuildModel?> DeleteAsync(ulong id, EdgeDBClient client) => (await QueryBuilder.Delete<GuildModel>().Filter(model => model.Id == id).ExecuteAsync(client, Capabilities.Modifications)).FirstOrDefault();

        private static GuildModel CombineMembers(GuildModel guild, IEnumerable<GuildMemberModel> members)
        {
            guild._members.AddRange(members);
            return guild;
        }
    }
}
