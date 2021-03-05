using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class VoiceBan : BaseCommandModule
	{
		public Database Database { private get; set; }

		[Command("guild_count")]
		public async Task Guild(CommandContext context) => await Program.SendMessage(context, (await Database.Guilds.CountAsync()).ToString());


		[Command("voiceban"), Description("Prevents the victim from joining voice channels. Good to use when someone is switching between channels or spamming unmutting and muting."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.MuteMembers), Aliases("voice_ban", "vb"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string voiceBanReason = Constants.MissingReason)
		{
			DiscordRole voiceBanRole = null;
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild != null) voiceBanRole = guild.VoiceBanRole.GetRole(context.Guild);
			if (voiceBanRole == null)
			{
				_ = await Program.SendMessage(context, Constants.MissingRole);
				return;
			}

			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been voicebanned by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. This means you cannot join voice channels. Reason: {Formatter.BlockCode(Formatter.Strip(voiceBanReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(voiceBanRole, voiceBanReason);
			}

			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user != null) user.IsVoiceBanned = false;

			_ = await Program.SendMessage(context, $"{victim.Mention} has been voicebanned{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(voiceBanReason))}", null, new UserMention(victim.Id));
		}

		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			using IServiceScope scope = context.Services.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null) return;
			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user != null) user.IsVoiceBanned = false;

			DiscordRole voicebanRole = guild.VoiceBanRole.GetRole(context.Guild);
			if (voicebanRole == null) return;

			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been given free speech by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode("Voiceban complete!")}");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(voicebanRole, "Voiceban complete!");
			}
		}
	}
}
