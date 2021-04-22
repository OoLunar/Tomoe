using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Humanizer;

namespace Tomoe
{
	public static class ExtensionMethods
	{
		public static DiscordEmbedBuilder GenerateDefaultEmbed(this DiscordEmbedBuilder embedBuilder, CommandContext context, string title = null)
		{
			if (!string.IsNullOrEmpty(title)) embedBuilder.Title = title.Titleize();
			embedBuilder.Color = new DiscordColor("#7b84d1");
			if (context.Guild == null)
			{
				embedBuilder.Author = new()
				{
					Name = context.User.Username,
					IconUrl = context.User.AvatarUrl,
					Url = context.User.AvatarUrl
				};
			}
			else
			{
				embedBuilder.Author = new()
				{
					Name = context.Member.GetCommonName(),
					IconUrl = context.Member.AvatarUrl,
					Url = context.Member.AvatarUrl
				};
			}
			return embedBuilder;
		}

		public static string GetCommonName(this DiscordMember guildMember) => guildMember == null ? null : guildMember.Nickname ?? guildMember.Username;

		/// <summary>
		/// Attempts to retrieve the DiscordMember from cache, then the API if the cache does not have the member.
		/// </summary>
		/// <param name="discordGuild">The guild to get the DiscordMember from.</param>
		/// <param name="discordUserId">The id to search for in the DiscordGuild.</param>
		/// <returns>The DiscordMember from the DiscordGuild</returns>
		public static async Task<DiscordMember> GetMember(this ulong discordUserId, DiscordGuild discordGuild)
		{
			try
			{
				return discordGuild.Members.Values.FirstOrDefault(member => member.Id == discordUserId) ?? await discordGuild.GetMemberAsync(discordUserId);
			}
			catch (Exception)
			{
				// Exceptions are not our problem
				throw;
			}
		}

		public static DiscordRole GetRole(this ulong roleId, DiscordGuild guild) => roleId != 0 ? guild.GetRole(roleId) : null;
		public static bool HasPermission(this DiscordMember guildMember, Permissions permission) => !guildMember.Roles.Any() ? guildMember.Guild.EveryoneRole.HasPermission(permission) : guildMember.Roles.Any(role => role.HasPermission(permission));
		public static bool HasPermission(this DiscordRole role, Permissions permission) => role.Permissions.HasPermission(permission);
	}
}
