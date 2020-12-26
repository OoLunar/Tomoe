using System;
using DSharpPlus.Entities;

namespace Tomoe
{
	public static class ExtensionMethods
	{
		[Flags]
		public enum FilteringAction
		{
			CodeBlocksIgnore = 1,
			CodeBlocksEscape = 2,
			CodeBlocksZeroWidthSpace = 4
		}

		public static string Filter(this string modifyString, FilteringAction filteringAction = FilteringAction.CodeBlocksEscape)
		{
			if (string.IsNullOrEmpty(modifyString))
			{
				return null;
			}
			else if (filteringAction.HasFlag(FilteringAction.CodeBlocksZeroWidthSpace) || filteringAction.HasFlag(FilteringAction.CodeBlocksEscape))
			{
				return modifyString.Replace("`", "​`​"); // There are zero width spaces before and after the backtick.
			}
			else
			{
				return filteringAction.HasFlag(FilteringAction.CodeBlocksEscape) ? modifyString.Replace("\\", "\\\\").Replace("`", "\\`") : modifyString;
			}
		}

		public static string GetCommonName(this DiscordMember guildMember) => guildMember == null ? null : guildMember.Nickname ?? guildMember.Username;
	}
}
