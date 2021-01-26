using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Tomoe.Types;

namespace Tomoe.Commands.Moderation.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class Punishment : CheckBaseAttribute
	{

		public bool CanSelfPunish { get; private set; }

		public Punishment(bool canSelfPunish = true) => CanSelfPunish = canSelfPunish;

		public override async Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
		{
			if (help) return true;
			else if (context.User.Id == Program.Client.CurrentUser.Id)
			{
				_ = Program.SendMessage(context, Constants.SelfAction);
				return false;
			}
			else if (context.Message.MentionedUsers.Select(user => user.Id).Contains(context.Guild.OwnerId))
			{
				_ = Program.SendMessage(context, Constants.GuildOwner);
				return false;
			}
			else if (context.Message.MentionedUsers.Contains(context.User))
			{
				bool confirm = false;
				DiscordMessage message = Program.SendMessage(context, Constants.SelfPunishment);
				_ = new Queue(message, context.User, new(async eventArgs =>
				{
					if (eventArgs.Emoji == Queue.ThumbsUp) confirm = true;
					else if (eventArgs.Emoji == Queue.ThumbsDown)
					{
						_ = message.ModifyAsync($"{Formatter.Strike(message.Content)}\n{Formatter.Bold("[Notice: Aborting...]")}");
						confirm = false;
					}
				})).WaitForReaction();
				return confirm;
			}
			else return true;
		}

		public static async Task<bool> CheckUser(CommandContext context, DiscordMember victim)
		{
			if (context.User.Id == Program.Client.CurrentUser.Id)
			{
				_ = Program.SendMessage(context, Constants.SelfAction);
				return false;
			}
			else if (context.User.Id == context.Guild.OwnerId)
			{
				_ = Program.SendMessage(context, Constants.GuildOwner);
				return false;
			}
			else if (context.Message.MentionedUsers.Contains(context.User))
			{
				bool confirm = false;
				DiscordMessage message = Program.SendMessage(context, Constants.SelfPunishment);
				_ = new Queue(message, context.User, new(async eventArgs =>
				{
					if (eventArgs.Emoji == Queue.ThumbsUp) confirm = true;
					else if (eventArgs.Emoji == Queue.ThumbsDown)
					{
						_ = message.ModifyAsync($"{Formatter.Strike(message.Content)}\n{Formatter.Bold("[Notice: Aborting...]")}");
						confirm = false;
					}
				})).WaitForReaction();
				return confirm;
			}
			else if (context.Member.Hierarchy <= victim.Hierarchy)
			{
				_ = Program.SendMessage(context, Constants.Hierarchy);
				return false;
			}
			else return true;
		}
	}
}
