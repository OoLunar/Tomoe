using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Npgsql;

namespace OoLunar.Tomoe.Database
{
    public interface IExpirableModel<TSelf, TId> where TId : ISpanParsable<TId>
    {
        public static abstract string TableName { get; }

        public TId Id { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }

        public static abstract bool TryParse(NpgsqlDataReader reader, [NotNullWhen(true)] out TSelf? expirable);
        public static abstract ValueTask<bool> ExpireAsync(TSelf expirable, IServiceProvider serviceProvider);
    }
}
