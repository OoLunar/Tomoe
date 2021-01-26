using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class MemeBan : BaseCommandModule
	{
		[Command("antimeme"), Description("Prevents the victim from linking embeds, sending files or reacting to messages. All they can do is send and read messages. This is the command to use when someone is constantly spamming reactions onto messages or sending a bunch of images."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("anti_meme", "meme_ban", "memeban", "nomeme", "no_meme"), Punishment(true)]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string antimemeReason = Program.MissingReason)
		{
			DiscordRole antimemeRole = Program.Database.Guild.AntimemeRole(context.Guild.Id).GetRole(context.Guild);
			if (antimemeRole == null)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) await guildVictim.SendMessageAsync($"You've been antimemed by **{context.User.Mention}** from **{context.Guild.Name}**. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: {Formatter.BlockCode(Formatter.Strip(antimemeReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(antimemeRole, antimemeReason);
			}

			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, true);
			_ = Program.SendMessage(context, $"{victim.Mention} has been antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(antimemeReason))}", null, new UserMention(victim.Id));
		}
	}
}
