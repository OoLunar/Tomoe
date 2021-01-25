using DSharpPlus.Entities;

namespace Tomoe
{
	public static class ExtensionMethods
	{
		public static string GetCommonName(this DiscordMember guildMember) => guildMember == null ? null : guildMember.Nickname ?? guildMember.Username;
		public static DiscordRole GetRole(this ulong? roleId, DiscordGuild guild) => roleId.HasValue ? guild.GetRole(roleId.Value) : null;
	}
}
