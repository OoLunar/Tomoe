using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Antimeme : BaseCommandModule
	{
		[Command("antimeme"), Description("Prevents the victim from linking embeds, sending files or reacting to messages. All they can do is send and read messages. This is the command to use when someone is constantly spamming reactions onto messages or sending a bunch of images."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("anti_meme", "meme_ban", "memeban", "nomeme", "no_meme"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string antimemeReason = Constants.MissingReason)
		{
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			DiscordRole antimemeRole = guild.AntimemeRole.GetRole(context.Guild);
			if (antimemeRole == null)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been antimemed by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: {Formatter.BlockCode(Formatter.Strip(antimemeReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(antimemeRole, antimemeReason);
			}

			GuildUser user = guild.Users.First(user => user.Id == victim.Id);
			user.IsAntimemed = true;

			_ = await Program.SendMessage(context, $"{victim.Mention} has been antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(antimemeReason))}", null, new UserMention(victim.Id));
		}
	}
}
