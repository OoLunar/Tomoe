using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Types;

namespace Tomoe.Commands.Moderation
{
	public class Kick : BaseCommandModule
	{
		[Command("kick"), Description("Kicks people from the guild, sending them off with a private message."), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers)]
		public async Task KickUser(CommandContext context, [Description("The person to be kicked.")] DiscordUser victim, [Description("(Optional) Should prompt to confirm with the self kick")] bool confirmed = false, [Description("(Optional) The reason why the person is being kicked.")][RemainingText] string kickReason = Program.MissingReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}
			else if (victim == context.User && !confirmed)
			{
				DiscordMessage discordMessage = Program.SendMessage(context, "**[Notice: You're about to kick yourself. Are you sure about this?]**");
				_ = new Queue(discordMessage, context.User, new(async eventArgs =>
				{
					if (eventArgs.Emoji == Queue.ThumbsUp) await context.Member.RemoveAsync(kickReason);
					else if (eventArgs.Emoji == Queue.ThumbsDown) _ = Program.SendMessage(context, "Aborting...");
				}));
				return;
			}

			try
			{
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
				try
				{
					if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy || guildVictim.Hierarchy >= context.Member.Hierarchy)
					{
						_ = Program.SendMessage(context, Program.Hierarchy);
						return;
					}
					else if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been kicked by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{kickReason.Filter() ?? Program.MissingReason}\n```");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RemoveAsync(kickReason);
			}
			catch (NotFoundException)
			{
				_ = Program.SendMessage(context, $"Failed to kick {victim.Mention} since they aren't in the guild. Kick Reason:\n```{kickReason.Filter() ?? Program.MissingReason}```", null, new UserMention(victim.Id));
				return;
			}
			_ = Program.SendMessage(context, $"{victim.Mention} has been kicked. Reason: ```\n{kickReason.Filter() ?? Program.MissingReason}\n```", null, new UserMention(victim.Id));
		}

		[Command("kick"), RequireGuild]
		public async Task KickUser(CommandContext context, [Description("The person to be kicked.")] DiscordUser victim) => KickUser(context, victim, default, default);

		[Command("kick"), RequireGuild]
		public async Task KickUsers(CommandContext context, [Description("(Optional) The reason why people are being kicked.")] string kickReason = Program.MissingReason, [Description("The people to be kicked.")] params DiscordMember[] victims)
		{
			List<IMention> mentions = new();
			foreach (DiscordMember victim in victims)
			{
				if (victim == context.Client.CurrentUser)
				{
					_ = Program.SendMessage(context, Program.SelfAction);
					return;
				}
				else if (victim == context.User)
				{
					DiscordMessage discordMessage = Program.SendMessage(context, "**[Notice: You're about to kick yourself. Are you sure about this?]**");
					_ = new Queue(discordMessage, context.User, new(async eventArgs =>
					{
						if (eventArgs.Emoji == Queue.ThumbsUp) await context.Member.RemoveAsync(kickReason);
						else if (eventArgs.Emoji == Queue.ThumbsDown) _ = Program.SendMessage(context, "Aborting...");
					}));
					return;
				}

				try
				{
					DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
					if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy || guildVictim.Hierarchy >= context.Member.Hierarchy)
					{
						_ = Program.SendMessage(context, Program.Hierarchy);
						return;
					}
					else if (!victim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been kicked by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{kickReason.Filter() ?? Program.MissingReason}\n```");
				}
				catch (NotFoundException) { }
				catch (BadRequestException) { }
				catch (UnauthorizedException) { }
				await victim.RemoveAsync(kickReason);
				mentions.Add(new UserMention(victim.Id));
			}
			_ = Program.SendMessage(context, $"Successfully masskicked {string.Join<DiscordMember>(", ", victims)}. Reason: ```\n{kickReason.Filter()}\n```", null, victims.Select(user => new UserMention(user.Id) as IMention).ToArray());
		}

		[Command("kick"), RequireGuild]
		public async Task KickUsers(CommandContext context, [Description("The people to be kicked.")] params DiscordMember[] victims) => KickUsers(context, default, victims);
	}
}
