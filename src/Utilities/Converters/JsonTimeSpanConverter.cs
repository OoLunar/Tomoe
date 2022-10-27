using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Tomoe.Utilities.Converters
{
    /// <summary>
    /// This is for System.Text.Json, not DSharpPlus!
    /// </summary>
    public class JsonTimeSpanConverter : JsonConverter<TimeSpan>
    {
        private static readonly Regex TimeSpanRegex = new(@"^(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        ///<inheritdoc/>
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Parse(reader.GetString());

        ///<inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(format: null, CultureInfo.InvariantCulture));

        public static TimeSpan Parse(string value)
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

        public static bool TryParse(string value, out TimeSpan? timeSpan)
        {
            if (value == "0")
            {
                timeSpan = TimeSpan.Zero;
                return true;
            }

            if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            {
                timeSpan = null;
                return false;
            }

            value = value.ToLowerInvariant();

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan result))
            {
                timeSpan = result;
                return true;
            }

            string[] gps = new string[] { "days", "hours", "minutes", "seconds" };
            Match mtc = TimeSpanRegex.Match(value);
            if (!mtc.Success)
            {
                timeSpan = null;
                return false;
            }

            int d = 0;
            int h = 0;
            int m = 0;
            int s = 0;
            foreach (string gp in gps)
            {
                string gpc = mtc.Groups[gp].Value;
                if (string.IsNullOrWhiteSpace(gpc))
                {
                    continue;
                }

                char gpt = gpc[^1];
                int.TryParse(gpc.AsSpan(0, gpc.Length - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int val);
                switch (gpt)
                {
                    case 'd':
                        d = val;
                        break;

                    case 'h':
                        h = val;
                        break;

                    case 'm':
                        m = val;
                        break;

                    case 's':
                        s = val;
                        break;
                    default:
                        throw new ArgumentException($"Unknown unit '{gpt}' when parsing TimeSpan");
                }
            }
            timeSpan = new TimeSpan(d, h, m, s);
            return true;
        }
    }
}
