
namespace Tomoe.Utils.Converters
{
    using System;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This is for System.Text.Json, not DSharpPlus!
    /// </summary>
    public class JsonTimeSpanConverter : JsonConverter<TimeSpan>
    {
        private static readonly Regex TimeSpanRegex = new(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        ///<inheritdoc/>
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Convert(reader.GetString());

        ///<inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(format: null, CultureInfo.InvariantCulture));

        private static TimeSpan Convert(string value)
        {
            if (value == "0")
            {
                return TimeSpan.Zero;
            }
            else if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan result))
            {
                return result;
            }
            else
            {
                string[] groups = new[] { "days", "hours", "minutes", "seconds" };
                Match match = TimeSpanRegex.Match(value);
                if (!match.Success)
                {
                    throw new ArgumentException("Failed to parse TimeSpan!");
                }

                int d = 0;
                int h = 0;
                int m = 0;
                int s = 0;
                foreach (string group in groups)
                {
                    string groupCapture = match.Groups[group].Value;
                    if (string.IsNullOrWhiteSpace(groupCapture))
                    {
                        continue;
                    }

                    char captureChar = groupCapture[^1];
                    int.TryParse(groupCapture[..groupCapture.Length], NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue);
                    switch (captureChar)
                    {
                        case 'd':
                            d = intValue;
                            break;

                        case 'h':
                            h = intValue;
                            break;

                        case 'm':
                            m = intValue;
                            break;

                        case 's':
                            s = intValue;
                            break;
                        default:
                            throw new ArgumentException($"Unknown unit '{captureChar}' when parsing TimeSpan");
                    }
                }
                return new TimeSpan(d, h, m, s);
            }
        }
    }
}
