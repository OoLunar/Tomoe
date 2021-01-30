using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class Kick : BaseCommandModule
	{
		[Command("kick"), Description("Kicks people from the guild, sending them off with a private message."), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Punishment]
		public async Task User(CommandContext context, [Description("The person to be kicked.")] DiscordMember victim, [Description("(Optional) The reason why the person is being kicked."), RemainingText] string kickReason = Constants.MissingReason)
		{
			bool sentDm = false;
			if (victim != null && !victim.IsBot) try
				{
					_ = await victim.SendMessageAsync($"You've been kicked by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(kickReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
			await victim.RemoveAsync(kickReason);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been {(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(kickReason))}", null, new UserMention(victim.Id));
		}

		[Command("kick"), RequireGuild]
		public async Task Group(CommandContext context, [Description("(Optional) The reason why people are being kicked.")] string kickReason = Constants.MissingReason, [Description("The people to be kicked.")] params DiscordMember[] victims)
		{
			foreach (DiscordMember victim in victims) if (await Punishment.CheckUser(context, await context.Guild.GetMemberAsync(victim.Id))) await User(context, victim, kickReason);
		}

		[Command("kick"), RequireGuild]
		public async Task Group(CommandContext context, [Description("The people to be kicked.")] params DiscordMember[] victims) => await Group(context, default, victims);
	}
}
