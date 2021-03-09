using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class ChannelCreated
	{
		public static async Task Handler(DiscordClient _client, ChannelCreateEventArgs eventArgs)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				DiscordRole muteRole = guild.MuteRole.GetRole(eventArgs.Guild);
				if (muteRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, Moderation.Config.RoleAction.Mute, muteRole);

				DiscordRole antimemeRole = guild.AntimemeRole.GetRole(eventArgs.Guild);
				if (antimemeRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, Moderation.Config.RoleAction.Antimeme, antimemeRole);

				DiscordRole voicebanRole = guild.VoicebanRole.GetRole(eventArgs.Guild);
				if (voicebanRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, Moderation.Config.RoleAction.Voiceban, voicebanRole);
			}
		}
	}
}
