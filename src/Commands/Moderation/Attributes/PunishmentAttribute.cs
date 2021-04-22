using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Utils.Exceptions;
using Tomoe.Utils.Types;

namespace Tomoe.Commands.Moderation.Attributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class Punishment : CheckBaseAttribute
	{
		/// <summary>
		/// If the user is allowed to use the moderator command on themself.
		/// </summary>
		public bool CanSelfPunish { get; private set; } = true;

		/// <summary>
		/// Inserts safety checks to test if the user is allowed to execute the command on the victim. This attribute should be paired with the <see cref="RequireGuildAttribute"/> attribute.
		/// </summary>
		/// <param name="canSelfPunish">If the user is allowed to use the moderator command on themself.</param>
		public Punishment(bool canSelfPunish = true) => CanSelfPunish = canSelfPunish;

		/// <summary>
		/// Executed when DSharpPlus is doing attribute checks on the command before running.
		/// </summary>
		/// <param name="context">Used to get the user's id, mentioned users and their hierarchy if the guild is not null.</param>
		/// <param name="help">If the command is being executed for the help menu.</param>
		public override async Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
		{
			if (help) return true;
			else if (context.Message.MentionedUsers.Contains(Program.Client.CurrentUser))
			{
				_ = await Program.SendMessage(context, Constants.SelfBotAction);
				return false;
			}

			bool canExecute = true;

			if (CanSelfPunish && context.Message.MentionedUsers.Contains(context.User))
			{
				DiscordMessage confirmSelfPunishment = await Program.SendMessage(context, Formatter.Bold("[Notice: You're about to punish yourself. Do you still want to continue?]"));
				await new Queue(confirmSelfPunishment, context.User, new(async eventArgs =>
				{
					if (eventArgs.TimedOut || eventArgs.MessageReactionAddEventArgs.Emoji == Constants.ThumbsDown)
					{
						_ = await confirmSelfPunishment.ModifyAsync(Formatter.Strike(confirmSelfPunishment.Content) + '\n' + Formatter.Bold("[Notice: Aborting!]"));
					}
				})).WaitForReaction();
			}
			else
			{
				// Did not put it in a seperate else if statement due to the possibility that the guild is not null but no hierarchy is preventing the command from being executed.
				if (!context.Channel.IsPrivate && context.Guild != null)
				{
					foreach (DiscordUser discordUser in context.Message.MentionedUsers)
					{
						DiscordMember discordMember = await discordUser.Id.GetMember(context.Guild);
						if (discordMember.Hierarchy >= context.Member.Hierarchy || discordMember.Id == context.Guild.OwnerId)
						{
							throw new HierarchyException();
						}
					}
				}
			}
			return canExecute;
		}

		/// <summary>
		/// Inserts safety checks to test if the user is allowed to execute the command on the victim.
		/// </summary>
		/// <param name="context">Used to get the user's id, mentioned users and their hierarchy if the guild is not null.</param>
		/// <param name="victim">Who to preform the checks on.</param>
		/// <returns><c>true</c> if the command can be executed, otherwise <c>false</c>.</returns>
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
					if (eventArgs.TimedOut || eventArgs.MessageReactionAddEventArgs.Emoji == Constants.ThumbsDown)
					{
						_ = await confirmSelfPunishment.ModifyAsync(Formatter.Strike(confirmSelfPunishment.Content) + '\n' + Formatter.Bold("[Notice: Aborting!]"));
					}
					else if (eventArgs.MessageReactionAddEventArgs.Emoji == Constants.ThumbsUp)
					{
						confirm = true;
					}
				})).WaitForReaction();
				return confirm;
			}
			else return true;
		}
	}
}
