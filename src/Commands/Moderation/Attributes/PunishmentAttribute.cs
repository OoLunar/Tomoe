using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            if (help)
            {
                return true;
            }
            else if (context.Guild == null)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Command can only be used in a guild!"));
                return false;
            }

            List<DiscordMember> mentionedMembers = new();
            // Spltting here due to https://github.com/DSharpPlus/DSharpPlus/issues/886
            DiscordMember[] collection = await GetMembersAsync(context, context.RawArgumentString.Split(' '));
            if (collection != null && collection.Length != 0)
            {
                mentionedMembers.AddRange(collection);
                foreach (DiscordMember member in mentionedMembers)
                {
                    // TODO: Get guild staff roles and make sure the member isn't staff.
                    if (member.Hierarchy >= context.Member.Hierarchy || member.IsOwner)
                    {
                        throw new HierarchyException();
                    }
                }
            }

            if (CanSelfPunish && context.Message.MentionedUsers.Contains(context.User))
            {
                DiscordMessage confirmSelfPunishment = await Program.SendMessage(context, Formatter.Bold("[Notice: You're about to punish yourself. Do you still want to continue?]"));
                await new Queue(confirmSelfPunishment, context.User, new(async eventArgs =>
                {
                    if (eventArgs.TimedOut || eventArgs.MessageReactionAddEventArgs.Emoji == Constants.ThumbsDown)
                    {
                        await confirmSelfPunishment.ModifyAsync(Formatter.Strike(confirmSelfPunishment.Content) + '\n' + Formatter.Bold("[Notice: Aborting!]"));
                    }
                })).WaitForReaction();
            }

            return true;
        }

        private async Task<DiscordMember[]> GetMembersAsync(CommandContext context, IReadOnlyList<string> discordUsers)
        {
            List<DiscordMember> discordMembers = new();
            IArgumentConverter<DiscordMember> converter = new DiscordMemberConverter();
            foreach (string discordUserId in discordUsers)
            {
                try
                {
                    Optional<DiscordMember> optionalDiscordMember = await converter.ConvertAsync(discordUserId.ToString(), context);
                    if (optionalDiscordMember.HasValue)
                    {
                        discordMembers.Add(optionalDiscordMember.Value);
                    }
                }
                catch (NotFoundException) { }
            }
            return discordMembers.ToArray();
        }
    }
}