using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildAvailable
	{
		private static readonly ILogger _logger = Log.ForContext<GuildAvailable>();

		public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null)
			{
				guild = new(eventArgs.Guild.Id);
				_ = database.Guilds.Add(guild);
				_ = await database.SaveChangesAsync();
			}
			else
			{
				DiscordRole muteRole = guild.MuteRole.GetRole(eventArgs.Guild);
				DiscordRole antimemeRole = guild.AntimemeRole.GetRole(eventArgs.Guild);
				DiscordRole voicebanRole = guild.VoicebanRole.GetRole(eventArgs.Guild);
				if (muteRole != null) Moderation.Config.FixPermissions(eventArgs.Guild, Moderation.Config.RoleAction.Mute, muteRole);
				if (antimemeRole != null) Moderation.Config.FixPermissions(eventArgs.Guild, Moderation.Config.RoleAction.Antimeme, antimemeRole);
				if (voicebanRole != null) Moderation.Config.FixPermissions(eventArgs.Guild, Moderation.Config.RoleAction.Voiceban, voicebanRole);
			}
			_logger.Information($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
		}
	}
}
