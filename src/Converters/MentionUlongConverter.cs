using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Tomoe.Converters
{
	// https://github.com/Naamloos/ModCore/blob/7ef9324a2265ea2dd5434ea9dd0db602590fdab3/ModCore/Logic/MentionUlongConverter.cs
	[DisplayName("Mention or user id.")]
	public class MentionUlongConverter : IArgumentConverter<ulong>
	{
		public static readonly Regex MentionRegex = new(@"^<@!?(?<ID>[0-9]+)>$", RegexOptions.Compiled);

		public Task<Optional<ulong>> ConvertAsync(string value, CommandContext context)
		{
			if (ulong.TryParse(value, out ulong result))
			{
				return Task.FromResult(new Optional<ulong>(result));
			}

			Group group = MentionRegex.Match(value).Groups["ID"];
			return Task.FromResult(group.Success && ulong.TryParse(group.ValueSpan, out ulong mention) ? new Optional<ulong>(mention) : default);
		}
	}
}
