using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

namespace OoLunar.Tomoe
{
    public static partial class TextCommandUtilities
    {
        [GeneratedRegex(@"<@&(\d+)>", RegexOptions.Compiled)] private static partial Regex _roleMentionRegex();
        [GeneratedRegex(@"<@!?(\d+)>", RegexOptions.Compiled)] private static partial Regex _userMentionRegex();
        [GeneratedRegex(@"<#(\d+)>", RegexOptions.Compiled)] private static partial Regex _channelMentionRegex();

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern MessageCreateEventArgs _messageCreateEventArgsConstructor();
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Message")] private static extern void _messageCreateEventArgsMessageSetter(MessageCreateEventArgs messageCreateEventArgs, DiscordMessage message);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedUsers")] private static extern void _messageCreateEventArgsMentionedUsersSetter(MessageCreateEventArgs messageCreateEventArgs, IReadOnlyList<DiscordUser> mentionedUsers);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedRoles")] private static extern void _messageCreateEventArgsMentionedRolesSetter(MessageCreateEventArgs messageCreateEventArgs, IReadOnlyList<DiscordRole> mentionedRoles);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedChannels")] private static extern void _messageCreateEventArgsMentionedChannelsSetter(MessageCreateEventArgs messageCreateEventArgs, IReadOnlyList<DiscordChannel> mentionedChannels);

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern DiscordMessage _messageConstructor();
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Content")] private static extern void _messageContentSetter(DiscordMessage message, string content);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ChannelId")] private static extern void _messageChannelIdSetter(DiscordMessage message, ulong channelId);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Author")] private static extern void _messageAuthorSetter(DiscordMessage message, DiscordUser author);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mentionedUsers")] private static extern ref List<DiscordUser> _messageMentionedUsersSetter(DiscordMessage message);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mentionedRoles")] private static extern ref List<DiscordRole> _messageMentionedRolesSetter(DiscordMessage message);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mentionedChannels")] private static extern ref List<DiscordChannel> _messageMentionedChannelsSetter(DiscordMessage message);

        public static MessageCreateEventArgs CreateFakeMessageEventArgs(TextCommandContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            MessageCreateEventArgs messageCreateEventArgs = _messageCreateEventArgsConstructor();
            _messageCreateEventArgsMessageSetter(messageCreateEventArgs, context.Message);
            _messageCreateEventArgsMentionedUsersSetter(messageCreateEventArgs, context.Message.MentionedUsers);
            _messageCreateEventArgsMentionedRolesSetter(messageCreateEventArgs, context.Message.MentionedRoles);
            _messageCreateEventArgsMentionedChannelsSetter(messageCreateEventArgs, context.Message.MentionedChannels);

            return messageCreateEventArgs;
        }

        public static MessageCreateEventArgs CreateFakeMessageEventArgs(CommandContext context, string content)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

            DiscordMessage message = CreateFakeMessage(context, content);
            MessageCreateEventArgs messageCreateEventArgs = _messageCreateEventArgsConstructor();
            _messageCreateEventArgsMessageSetter(messageCreateEventArgs, message);
            _messageCreateEventArgsMentionedUsersSetter(messageCreateEventArgs, message.MentionedUsers);
            _messageCreateEventArgsMentionedRolesSetter(messageCreateEventArgs, message.MentionedRoles);
            _messageCreateEventArgsMentionedChannelsSetter(messageCreateEventArgs, message.MentionedChannels);

            return messageCreateEventArgs;
        }

        public static DiscordMessage CreateFakeMessage(CommandContext context, string content)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

            List<DiscordRole> roleMentions = [];
            List<DiscordUser> userMentions = [];
            List<DiscordChannel> channelMentions = [];
            if (context.Guild is not null)
            {
                MatchCollection roleMatches = _roleMentionRegex().Matches(content);
                foreach (Match match in roleMatches.Cast<Match>())
                {
                    if (ulong.TryParse(match.Groups[1].Value, out ulong roleId))
                    {
                        roleMentions.Add(context.Guild.GetRole(roleId));
                    }
                }

                MatchCollection userMatches = _userMentionRegex().Matches(content);
                foreach (Match match in userMatches.Cast<Match>())
                {
                    if (ulong.TryParse(match.Groups[1].Value, out ulong userId))
                    {
                        userMentions.Add(context.Client.GetUserAsync(userId).GetAwaiter().GetResult());
                    }
                }
            }

            MatchCollection channelMatches = _channelMentionRegex().Matches(content);
            foreach (Match match in channelMatches.Cast<Match>())
            {
                if (ulong.TryParse(match.Groups[1].Value, out ulong channelId))
                {
                    DiscordChannel? channel;
                    try
                    {
                        channel = context.Client.GetChannelAsync(channelId).GetAwaiter().GetResult();
                    }
                    catch (DiscordException)
                    {
                        continue;
                    }

                    channelMentions.Add(channel);
                }
            }

            DiscordMessage message = _messageConstructor();
            _messageContentSetter(message, content);
            _messageChannelIdSetter(message, context.Channel.Id);
            _messageAuthorSetter(message, context.User);
            _messageMentionedUsersSetter(message) = userMentions;
            _messageMentionedRolesSetter(message) = roleMentions;
            _messageMentionedChannelsSetter(message) = channelMentions;
            return message;
        }

        public static string GetDisplayName(this DiscordUser user)
        {
            if (user is DiscordMember member)
            {
                return member.DisplayName;
            }
            else if (!string.IsNullOrEmpty(user.GlobalName))
            {
                return user.GlobalName;
            }

            return user.Username;
        }

        public static string PluralizeCorrectly(this string str) => str.Length == 0 ? str : str[^1] switch
        {
            // Ensure it doesn't already end with `'s`
            's' when str.Length > 1 && str[^2] == '\'' => str,
            's' => str + '\'',
            '\'' => str + 's',
            _ => str + "'s"
        };
    }
}
