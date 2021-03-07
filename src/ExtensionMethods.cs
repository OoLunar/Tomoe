using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe
{
	public static class ExtensionMethods
	{
		public static string GetCommonName(this DiscordMember guildMember) => guildMember == null ? null : guildMember.Nickname ?? guildMember.Username;
		public static DiscordRole GetRole(this ulong roleId, DiscordGuild guild) => roleId != 0 ? guild.GetRole(roleId) : null;

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

		public static async Task<bool> IsAdmin(this DiscordMember guildMember, DiscordGuild discordGuild)
		{
			if (guildMember.HasPermission(Permissions.Administrator)) return true;
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
			return guild.AdminRoles.Cast<string>().Intersect(guildMember.Roles.Cast<string>()) != null;
		}

		public static bool HasPermission(this DiscordMember guildMember, Permissions permission) => !guildMember.Roles.Any() ? guildMember.Guild.EveryoneRole.HasPermission(permission) : guildMember.Roles.Any(role => role.HasPermission(permission));
		public static bool HasPermission(this DiscordRole role, Permissions permission) => role.Permissions.HasPermission(permission);
	}
}
