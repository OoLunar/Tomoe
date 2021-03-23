using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class ReactionRoles
	{
		/// <summary>
		/// Iterates through <see cref="Guild.ReactionRoles"> and assigns the specific role to whoever reacted.
		/// </summary>
		/// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
		/// <param name="eventArgs">Used to get the reaction and who's reacting.</param>
		public static async Task Handler(DiscordClient _client, MessageReactionAddEventArgs eventArgs)
		{
			if (eventArgs.User == Program.Client.CurrentUser) return;
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null) return;

			ReactionRole reactionRole = guild.ReactionRoles.FirstOrDefault(rr => rr.MessageId == eventArgs.Message.Id);
			if (reactionRole == null) return;

			DiscordRole discordRole = eventArgs.Guild.GetRole(reactionRole.RoleId);
			if (discordRole == null)
			{
				await eventArgs.Message.DeleteOwnReactionAsync(eventArgs.Emoji);
				_ = guild.ReactionRoles.Remove(reactionRole);
				return;
			}

			DiscordMember reactorMember = await eventArgs.User.Id.GetMember(eventArgs.Guild);
			await reactorMember.GrantRoleAsync(discordRole, "Reaction role");
		}
	}
}
