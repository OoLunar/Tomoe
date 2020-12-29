using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation
{
	public class Kick : BaseCommandModule
	{
		[Command("kick"), Description("Kicks people from the guild, sending them off with a private message."), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers)]
		public async Task KickUser(CommandContext context, [Description("The person to be kicked.")] DiscordUser victim, [Description("(Optional) The reason why the person is being kicked.")][RemainingText] string kickReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}

			try
			{
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
				try
				{
					if (guildVictim.Hierarchy > context.Guild.CurrentMember.Hierarchy)
					{
						_ = Program.SendMessage(context, Program.Hierarchy);
						return;
					}
					else if (!guildVictim.IsBot)
					{
						_ = await guildVictim.SendMessageAsync($"You've been kicked by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{kickReason.Filter() ?? Program.MissingReason}\n```");
					}
				}
				catch (UnauthorizedException) { }
				await guildVictim.RemoveAsync(kickReason);
			}
			catch (NotFoundException)
			{
				_ = Program.SendMessage(context, $"Failed to kick {victim.Mention} since they aren't in the guild. Kick Reason:\n```{kickReason.Filter() ?? Program.MissingReason}```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
				return;
			}
			_ = Program.SendMessage(context, $"{victim.Mention} has been kicked. Reason: ```\n{kickReason.Filter() ?? Program.MissingReason}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
		}

		[Command("kick"), RequireGuild]
		public async Task KickUser(CommandContext context, [Description("The person to be kicked.")] DiscordUser victim) => KickUser(context, victim, null);

		[Command("kick"), RequireGuild]
		public async Task KickUsers(CommandContext context, [Description("(Optional) The reason why people are being kicked.")] string kickReason = Program.MissingReason, [Description("The people to be kicked.")] params DiscordUser[] victims)
		{
			foreach (DiscordUser victim in victims) _ = KickUser(context, victim, kickReason);
			_ = Program.SendMessage(context, $"Successfully masskicked {string.Join<DiscordUser>(", ", victims)}. Reason: ```\n{kickReason.Filter()}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, victims.Select(user => new UserMention(user.Id) as IMention).ToList());
		}

		[Command("kick"), RequireGuild]
		public async Task KickUsers(CommandContext context, [Description("The people to be kicked.")] params DiscordUser[] victims) => KickUsers(context, default, victims);
	}
}
