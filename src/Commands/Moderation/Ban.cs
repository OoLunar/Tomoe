using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class Ban : BaseCommandModule
	{
		[Command("ban"), Description("Bans people from the guild, sending them off with a private message."), RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Punishment]
		public async Task User(CommandContext context, [Description("The person to be banned.")] DiscordUser victim, [Description("(Optional) Removed the victim's messages from the pass `x` days.")] int pruneDays = 7, [Description("(Optional) The reason why the person is being banned."), RemainingText] string banReason = Constants.MissingReason)
		{
			if (pruneDays < 7) pruneDays = 7;
			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been banned by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
			await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason);
			_ = Program.SendMessage(context, $"{victim.Mention} has been permanently banned{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}", null, new UserMention(victim.Id));
		}

		[Command("ban")]
		public async Task User(CommandContext context, [Description("The person to be banned.")] DiscordUser victim, [Description("(Optional) Removed the victim's messages from the pass `x` days.")] int pruneDays = 7) => User(context, victim, pruneDays, Constants.MissingReason);

		[Command("ban")]
		public async Task Group(CommandContext context, [Description("(Optional) Removed the victim's messages from the pass `x` days.")] int pruneDays = 7, [Description("(Optional) The reason why the people are being banned.")] string banReason = Constants.MissingReason, [Description("The people to be banned.")] params DiscordUser[] victims)
		{
			foreach (DiscordUser victim in victims) if (await Punishment.CheckUser(context, await context.Guild.GetMemberAsync(victim.Id))) await User(context, victim, pruneDays, banReason);
		}

		[Command("ban")]
		public async Task Group(CommandContext context, [Description("(Optional) The reason why the people are being banned.")] string banReason = Constants.MissingReason, [Description("The people to be banned.")] params DiscordUser[] victims) => Group(context, default, banReason, victims);

		[Command("ban")]
		public async Task Group(CommandContext context, [Description("The people to be banned.")] params DiscordUser[] victims) => Group(context, default, default, victims);
	}
}
