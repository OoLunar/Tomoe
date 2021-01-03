using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext;

namespace Tomoe
{
	public class ExpandedTimeSpan
	{
		public TimeSpan TimeSpan;
		public override string ToString() => TimeSpan.ToString();
	}

	public class ExpandedTimeSpanConverter : IArgumentConverter<ExpandedTimeSpan>
	{

		private static readonly Regex _timeSpanRegex = new(@"^(?<years>\d{1,2}y\s*)?(?<months>\d{1,2}M\s*)?(?<weeks>\d{1,2}w\s*)?(?<days>\d+d\s*)?(?<hours>\d{1,2}h\s*)?(?<minutes>\d{1,2}m\s*)?(?<seconds>\d{1,2}s\s*)?$", RegexOptions.ECMAScript | RegexOptions.Compiled);

		public Task<Optional<ExpandedTimeSpan>> ConvertAsync(string value, CommandContext ctx)
		{
			ExpandedTimeSpan expandedTimeSpan = new();
			if (value == "0")
			{
				expandedTimeSpan.TimeSpan = TimeSpan.Zero;
				return Task.FromResult(Optional.FromValue(expandedTimeSpan));
			}

			if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
			{
				return Task.FromResult(Optional.FromNoValue<ExpandedTimeSpan>());
			}

			if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var result))
			{
				expandedTimeSpan.TimeSpan = result;
				return Task.FromResult(Optional.FromValue(expandedTimeSpan));
			}

			string[] groups = new string[] { "years", "months", "weeks", "days", "hours", "minutes", "seconds" };
			Match match = _timeSpanRegex.Match(value);
			if (!match.Success)
			{
				return Task.FromResult(Optional.FromNoValue<ExpandedTimeSpan>());
			}

			int d = 0;
			int h = 0;
			int m = 0;
			int s = 0;
			foreach (string group in groups)
			{
				string groupCapture = match.Groups[group].Value;
				if (string.IsNullOrWhiteSpace(groupCapture)) continue;

				char gpt = groupCapture[^1];
				_ = int.TryParse(groupCapture[..groupCapture.Length], NumberStyles.Integer, CultureInfo.InvariantCulture, out int val);
				switch (gpt)
				{
					case 'y':
						d += val * 365;
						break;
					case 'M':
						d += (int)(val * 30.4375);
						break;
					case 'w':
						d += val * 7;
						break;
					case 'd':
						d += val;
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
				}
			}
			result = new TimeSpan(d, h, m, s);
			expandedTimeSpan.TimeSpan = result;
			return Task.FromResult(Optional.FromValue(expandedTimeSpan));
		}
	}
}
