namespace Tomoe.Commands.Moderation.Attributes
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Tomoe.Utils.Exceptions;
    using Tomoe.Utils.Types;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class Punishment : CheckBaseAttribute
    {
        private Regex UserRegex { get; }

        /// <summary>
        /// If the user is allowed to use the moderator command on themself.
        /// </summary>
        public bool CanSelfPunish { get; private set; } = true;

        /// <summary>
        /// Inserts safety checks to test if the user is allowed to execute the command on the victim. This attribute should be paired with the <see cref="RequireGuildAttribute"/> attribute.
        /// </summary>
        /// <param name="canSelfPunish">If the user is allowed to use the moderator command on themself.</param>
        public Punishment(bool canSelfPunish = true)
        {
            CanSelfPunish = canSelfPunish;
            UserRegex = new Regex("^<@\\!?(\\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript);
        }

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
            foreach (string discordUserId in discordUsers)
            {
                Optional<DiscordMember> optionalDiscordMember = await ConvertAsync(discordUserId, context);
                if (optionalDiscordMember.HasValue)
                {
                    discordMembers.Add(optionalDiscordMember.Value);
                }
            }
            return discordMembers.ToArray();
        }

        private async Task<Optional<DiscordMember>> ConvertAsync(string value, CommandContext context)
        {
            if (context.Guild == null)
            {
                return Optional.FromNoValue<DiscordMember>();
            }

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong result))
            {
                DiscordMember discordMember = await context.Guild.GetMemberAsync(result).ConfigureAwait(continueOnCapturedContext: false);
                return (discordMember != null) ? Optional.FromValue(discordMember) : Optional.FromNoValue<DiscordMember>();
            }

            Match match = UserRegex.Match(value);
            if (match.Success && ulong.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                DiscordMember discordMember2 = await context.Guild.GetMemberAsync(result).ConfigureAwait(continueOnCapturedContext: false);
                return (discordMember2 != null) ? Optional.FromValue(discordMember2) : Optional.FromNoValue<DiscordMember>();
            }
            bool cs = true;
            if (!cs)
            {
                value = value.ToLowerInvariant();
            }
            int num = value.IndexOf('#');
            string un = (num != -1) ? value.Substring(0, num) : value;
            string dv = (num != -1) ? value[(num + 1)..] : null;
            DiscordMember discordMember3 = context.Guild.Members.Values.Where((DiscordMember xm) => ((cs ? xm.Username : xm.Username.ToLowerInvariant()) == un && ((dv != null && xm.Discriminator == dv) || dv == null)) || (cs ? xm.Nickname : xm.Nickname?.ToLowerInvariant()) == value).FirstOrDefault();
            return (discordMember3 != null) ? Optional.FromValue(discordMember3) : Optional.FromNoValue<DiscordMember>();
        }
    }
}
