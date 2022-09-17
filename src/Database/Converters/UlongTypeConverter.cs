using System.Globalization;
using EdgeDB.TypeConverters;

namespace OoLunar.Tomoe.Database.Converters
{
    public sealed class UlongTypeConverter : EdgeDBTypeConverter<ulong, string>
    {
        public override ulong ConvertFrom(string? value) => value is null ? 0 : ulong.Parse(value, NumberStyles.Number, CultureInfo.InvariantCulture);
        public override string? ConvertTo(ulong value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
