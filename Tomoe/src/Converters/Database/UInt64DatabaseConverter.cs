using System.Globalization;
using EdgeDB.TypeConverters;

namespace OoLunar.Tomoe.Converters.Database
{
    public sealed class UInt64DatabaseConverter : EdgeDBTypeConverter<ulong, string>
    {
        public override string ConvertTo(ulong value) => value.ToString(CultureInfo.InvariantCulture);
        public override ulong ConvertFrom(string? value) => string.IsNullOrWhiteSpace(value) ? default : ulong.Parse(value, CultureInfo.InvariantCulture);
    }
}
