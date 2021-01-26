using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Humanizer;

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

		public static DiscordEmbedBuilder GenerateDefaultEmbed(this DiscordEmbedBuilder embedBuilder, CommandContext context, string title)
		{
			embedBuilder.Title = title.Humanize(LetterCasing.Title);
			embedBuilder.Color = new DiscordColor("#7b84d1");
			embedBuilder.Author = new()
			{
				Name = context.Member.GetCommonName(),
				IconUrl = context.Member.AvatarUrl,
				Url = context.Member.AvatarUrl
			};
			return embedBuilder;
		}
	}
}
