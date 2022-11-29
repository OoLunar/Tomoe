using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Tomoe.Converters
{
	[DisplayName("User or Channel Mention")]
	public class IMentionConverter : IArgumentConverter<IMention>
	{
		[SuppressMessage("Roslyn", "IDE0046", Justification = "Don't fall down the ternary operator rabbit hole")]
		public Task<Optional<IMention>> ConvertAsync(string value, CommandContext ctx)
		{
			if (value.StartsWith("<@&", true, CultureInfo.InvariantCulture))
			{
				return Task.FromResult(Optional.FromValue<IMention>(new RoleMention(ulong.Parse(value.AsSpan(3, value.Length - 4), NumberStyles.Number, CultureInfo.InvariantCulture))));
			}
			else if (value.StartsWith("<@!", true, CultureInfo.InvariantCulture))
			{
				return Task.FromResult(Optional.FromValue<IMention>(new UserMention(ulong.Parse(value.AsSpan(3, value.Length - 4), NumberStyles.Number, CultureInfo.InvariantCulture))));
			}
			else if (value.StartsWith("<@", true, CultureInfo.InvariantCulture))
			{
				return Task.FromResult(Optional.FromValue<IMention>(new UserMention(ulong.Parse(value.AsSpan(2, value.Length - 3), NumberStyles.Number, CultureInfo.InvariantCulture))));
			}
			else if (value == "@everyone")
			{
				return Task.FromResult(Optional.FromValue<IMention>(new EveryoneMention()));
			}
			else
			{
				return Task.FromResult(Optional.FromNoValue<IMention>());
			}
		}
	}
}
