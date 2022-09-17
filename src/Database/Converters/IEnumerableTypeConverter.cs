using System.Collections.Generic;
using System.Linq;
using EdgeDB.TypeConverters;

namespace OoLunar.Tomoe.Database.Converters
{
    public sealed class IEnumerableTypeConverter<T1, T2> : EdgeDBTypeConverter<IEnumerable<T1>, List<T2>>
    {
        public override IEnumerable<T1> ConvertFrom(List<T2>? value) => value?.Cast<T1>() ?? Enumerable.Empty<T1>();
        public override List<T2> ConvertTo(IEnumerable<T1>? value) => value?.Cast<T2>().ToList() ?? new List<T2>();
    }
}
