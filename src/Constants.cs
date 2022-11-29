using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DSharpPlus.Entities;

namespace Tomoe
{
	public static class Constants
	{
		public const string NoReasonSpecified = "**[Error]: No reason specified.**";
		public const string ZeroWidthSpace = "\u200B";

		public static string ToString(this IMention mention) => mention switch
		{
			RoleMention roleMention => $"<@&{roleMention.Id}>",
			UserMention userMention => $"<@{userMention.Id}>",
			EveryoneMention => "@everyone",
			_ => throw new ArgumentException("Invalid IMention type", nameof(mention))
		};

		[SuppressMessage("Roslyn", "IDE0046", Justification = "Don't fall down the ternary operator rabbit hole")]
		public static IMention ToMention(this string mention)
		{
			if (mention.StartsWith("<@&", true, CultureInfo.InvariantCulture))
			{
				return new RoleMention(ulong.Parse(mention.AsSpan(3, mention.Length - 4), NumberStyles.Number, CultureInfo.InvariantCulture));
			}
			else if (mention.StartsWith("<@", true, CultureInfo.InvariantCulture))
			{
				return new UserMention(ulong.Parse(mention.AsSpan(2, mention.Length - 3), NumberStyles.Number, CultureInfo.InvariantCulture));
			}
			else if (mention == "@everyone")
			{
				return new EveryoneMention();
			}
			else
			{
				throw new ArgumentException("Invalid mention string", nameof(mention));
			}
		}
	}
}
