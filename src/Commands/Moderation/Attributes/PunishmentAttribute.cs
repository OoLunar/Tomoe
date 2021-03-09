using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Utils.Types;

namespace Tomoe.Commands.Moderation.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class Punishment : CheckBaseAttribute
	{
		public bool CanSelfPunish { get; private set; } = true;
		public Punishment(bool canSelfPunish = true) => CanSelfPunish = canSelfPunish;

		public override async Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
		{
			if (help) return true;
			else if (context.User.Id == Program.Client.CurrentUser.Id)
			{
				_ = await Program.SendMessage(context, Constants.SelfBotAction);
				return false;
			}
			else if (context.Message.MentionedUsers.Select(user => user.Id).Contains(context.Guild.OwnerId))
			{
				_ = await Program.SendMessage(context, Constants.GuildOwner);
				return false;
			}
			else if (context.Message.MentionedUsers.Contains(context.User) && CanSelfPunish)
			{
				bool confirm = false;
				DiscordMessage confirmSelfPunishment = await Program.SendMessage(context, Formatter.Bold("[Notice: You're about to punish yourself. Do you still want to continue?]"));
				await new Queue(confirmSelfPunishment, context.User, new(async eventArgs =>
				{
					if (eventArgs.Emoji == Constants.ThumbsUp)
					{
						confirm = true;
					}
					else if (eventArgs.Emoji == Constants.ThumbsDown)
					{
						_ = await confirmSelfPunishment.ModifyAsync(Formatter.Strike(confirmSelfPunishment.Content) + '\n' + Formatter.Bold("[Notice: Aborting!]"));
					}
				})).WaitForReaction();
				return confirm;
			}
			else
			{
				return true;
			}
		}

		public static async Task<bool> CheckUser(CommandContext context, DiscordMember victim)
		{
			if (context.User.Id == Program.Client.CurrentUser.Id)
			{
				_ = await Program.SendMessage(context, Constants.SelfBotAction);
				return false;
			}
			else if (context.Message.MentionedUsers.Select(user => user.Id).Contains(context.Guild.OwnerId))
			{
				_ = await Program.SendMessage(context, Constants.GuildOwner);
				return false;
			}
			else if (context.Member.Hierarchy <= victim.Hierarchy)
			{
				_ = await Program.SendMessage(context, Constants.Hierarchy);
				return false;
			}
			else if (context.Message.MentionedUsers.Contains(context.User))
			{
				bool confirm = false;
				DiscordMessage confirmSelfPunishment = await Program.SendMessage(context, Formatter.Bold("[Notice: You're about to punish yourself. Do you still want to continue?]"));
				await new Queue(confirmSelfPunishment, context.User, new(async eventArgs =>
				{
					if (eventArgs.Emoji == Constants.ThumbsUp)
					{
						confirm = true;
					}
					else if (eventArgs.Emoji == Constants.ThumbsDown)
					{
						_ = await confirmSelfPunishment.ModifyAsync(Formatter.Strike(confirmSelfPunishment.Content) + '\n' + Formatter.Bold("[Notice: Aborting!]"));
					}
				})).WaitForReaction();
				return confirm;
			}
			else return true;
		}
	}
}
