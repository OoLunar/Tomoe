
namespace OoLunar.Tomoe.Database.Converters
{
    public sealed class UlongTypeConverter// : EdgeDBTypeConverter<ulong, long>
    {
        public ulong ConvertFrom(long value) => (ulong)value;
        public long ConvertTo(ulong value) => (long)value;
    }
}
