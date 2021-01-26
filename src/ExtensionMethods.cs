using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe
{
	public static class ExtensionMethods
	{
		public static string GetCommonName(this DiscordMember guildMember) => guildMember == null ? null : guildMember.Nickname ?? guildMember.Username;
		public static DiscordRole GetRole(this ulong? roleId, DiscordGuild guild) => roleId.HasValue ? guild.GetRole(roleId.Value) : null;
		public static DiscordMember GetMember(this DiscordUser user, DiscordGuild guild)
		{
			try { return guild.GetMemberAsync(user.Id).GetAwaiter().GetResult(); }
			catch (NotFoundException) { return null; }
		}
	}
}
