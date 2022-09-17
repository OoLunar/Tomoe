using EdgeDB.TypeConverters;

namespace OoLunar.Tomoe.Database.Converters
{
    public sealed class UlongTypeConverter : EdgeDBTypeConverter<ulong, long>
    {
        public override ulong ConvertFrom(long value) => (ulong)value;
        public override long ConvertTo(ulong value) => (long)value;
    }
}
