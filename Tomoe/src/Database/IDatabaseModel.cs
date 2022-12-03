using System.Threading.Tasks;
using EdgeDB;

namespace OoLunar.Tomoe.Database
{
    public interface IDatabaseModel<TType, TId> where TType : IDatabaseModel<TType, TId>
    {
        TId Id { get; }

        static abstract Task<TType?> CreateAsync(TType entity, EdgeDBClient client);
        static abstract Task<TType?> UpdateAsync(TType entity, EdgeDBClient client);
        static abstract Task<TType?> FindAsync(TId id, EdgeDBClient client);
        static abstract Task<TType?> DeleteAsync(TId id, EdgeDBClient client);
    }
}
