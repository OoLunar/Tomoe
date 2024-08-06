using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe
{
    public static partial class TextCommandUtilities
    {
        [GeneratedRegex(@"<@&(\d+)>", RegexOptions.Compiled)] private static partial Regex _roleMentionRegex();
        [GeneratedRegex(@"<@!?(\d+)>", RegexOptions.Compiled)] private static partial Regex _userMentionRegex();
        [GeneratedRegex(@"<#(\d+)>", RegexOptions.Compiled)] private static partial Regex _channelMentionRegex();

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern MessageCreatedEventArgs _messageCreateEventArgsConstructor();
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Message")] private static extern void _messageCreateEventArgsMessageSetter(MessageCreatedEventArgs messageCreateEventArgs, DiscordMessage message);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedUsers")] private static extern void _messageCreateEventArgsMentionedUsersSetter(MessageCreatedEventArgs messageCreateEventArgs, IReadOnlyList<DiscordUser> mentionedUsers);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedRoles")] private static extern void _messageCreateEventArgsMentionedRolesSetter(MessageCreatedEventArgs messageCreateEventArgs, IReadOnlyList<DiscordRole> mentionedRoles);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MentionedChannels")] private static extern void _messageCreateEventArgsMentionedChannelsSetter(MessageCreatedEventArgs messageCreateEventArgs, IReadOnlyList<DiscordChannel> mentionedChannels);
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern DiscordMessage _messageConstructor();
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)] private static extern DiscordMessage _messageConstructor(DiscordMessage message);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Content")] private static extern void _messageContentSetter(DiscordMessage message, string content);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Channel")] private static extern void _messageChannelSetter(DiscordMessage message, DiscordChannel channel);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_ChannelId")] private static extern void _messageChannelIdSetter(DiscordMessage message, ulong channelId);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_guildId")] private static extern void _messageGuildIdSetter(DiscordMessage message, ulong? guildId);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Author")] private static extern void _messageAuthorSetter(DiscordMessage message, DiscordUser author);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mentionedUsers")] private static extern ref List<DiscordUser> _messageMentionedUsersSetter(DiscordMessage message);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mentionedRoles")] private static extern ref List<DiscordRole> _messageMentionedRolesSetter(DiscordMessage message);
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "mentionedChannels")] private static extern ref List<DiscordChannel> _messageMentionedChannelsSetter(DiscordMessage message);

        public static async Task<Optional<T>> ExecuteAsync<T>(this ITextArgumentConverter<T> converter, CommandContext context, string value)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
            TextConverterContext converterContext = new()
            {
                Channel = context.Channel,
                Command = context.Command,
                Extension = context.Extension,
                RawArguments = value,
                ServiceScope = context.ServiceProvider.CreateAsyncScope(),
                Splicer = context.Extension.GetProcessor<TextCommandProcessor>().Configuration.TextArgumentSplicer,
                User = context.User
            };

            // Move to the first argument (value)
            converterContext.NextArgument();

            DiscordMessage? message = context is TextCommandContext textCommandContext ? textCommandContext.Message : _messageConstructor();
            return await converter.ConvertAsync(converterContext, await CreateFakeMessageEventArgsAsync(context, message, value));
        }

        /// <summary>
        /// Creates a shallow copy of a Discord message.
        /// </summary>
        /// <param name="message">The message to copy.</param>
        /// <returns>A shallow copy of the message.</returns>
        public static DiscordMessage Copy(this DiscordMessage message) => _messageConstructor(message);

        /// <summary>
        /// Creates a fake message created event args object with the specified content.
        /// </summary>
        /// <param name="context">The command context to use for the message.</param>
        /// <param name="message">The message to use as a base for the new message.</param>
        /// <param name="content">The content of the new message.</param>
        /// <returns>The created message created event args object.</returns>
        public static async ValueTask<MessageCreatedEventArgs> CreateFakeMessageEventArgsAsync(CommandContext context, DiscordMessage message, string content)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(message, nameof(message));
            ArgumentException.ThrowIfNullOrWhiteSpace(content, nameof(content));

            // Create a copy of the message to modify to prevent modifying the original message.
            // By modifying the original message, we could potentially cause issues with the message cache.
            DiscordMessage messageCopy = message.Copy();

            // Modify the copied message to contain the contents that the user wants.
            await messageCopy.ModifyMessagePropertiesAsync(content, context.Client, context.User, context.Channel, context.Guild);

            // Create the message created event args.
            MessageCreatedEventArgs messageCreateEventArgs = _messageCreateEventArgsConstructor();
            _messageCreateEventArgsMessageSetter(messageCreateEventArgs, messageCopy);
            _messageCreateEventArgsMentionedUsersSetter(messageCreateEventArgs, messageCopy.MentionedUsers);
            _messageCreateEventArgsMentionedRolesSetter(messageCreateEventArgs, messageCopy.MentionedRoles);
            _messageCreateEventArgsMentionedChannelsSetter(messageCreateEventArgs, messageCopy.MentionedChannels);

            // Return the message created event args, which can be used for argument conversion or command execution.
            return messageCreateEventArgs;
        }

        /// <summary>
        /// Modifies the internal properties of a message, changing the content, author, channel, and mentions.
        /// </summary>
        /// <param name="message">The message to modify.</param>
        /// <param name="content">The new content of the message.</param>
        /// <param name="client">The client to use for fetching users and channels.</param>
        /// <param name="user">The new author of the message.</param>
        /// <param name="channel">The new channel of the message.</param>
        /// <param name="guild">The guild to use for fetching roles.</param>
        public static async ValueTask ModifyMessagePropertiesAsync(this DiscordMessage message, string content, DiscordClient client, DiscordUser user, DiscordChannel channel, DiscordGuild? guild = null)
        {
            List<DiscordRole> roleMentions = [];
            List<DiscordUser> userMentions = [];
            List<DiscordChannel> channelMentions = [];
            if (guild is not null)
            {
                MatchCollection roleMatches = _roleMentionRegex().Matches(content);
                foreach (Match match in roleMatches.Cast<Match>())
                {
                    if (ulong.TryParse(match.Groups[1].Value, out ulong roleId) && guild.GetRole(roleId) is DiscordRole role)
                    {
                        roleMentions.Add(role);
                    }
                }

                MatchCollection userMatches = _userMentionRegex().Matches(content);
                foreach (Match match in userMatches.Cast<Match>())
                {
                    if (ulong.TryParse(match.Groups[1].Value, out ulong userId))
                    {
                        userMentions.Add(await client.GetUserAsync(userId));
                    }
                }
            }

            MatchCollection channelMatches = _channelMentionRegex().Matches(content);
            foreach (Match match in channelMatches.Cast<Match>())
            {
                if (ulong.TryParse(match.Groups[1].Value, out ulong channelId))
                {
                    DiscordChannel? mentionedChannel;
                    try
                    {
                        mentionedChannel = await client.GetChannelAsync(channelId);
                    }
                    catch (DiscordException)
                    {
                        continue;
                    }

                    channelMentions.Add(mentionedChannel);
                }
            }

            _messageContentSetter(message, content);
            _messageChannelSetter(message, channel);
            _messageChannelIdSetter(message, channel.Id);
            _messageGuildIdSetter(message, channel.GuildId);
            _messageAuthorSetter(message, user);
            _messageMentionedUsersSetter(message) = userMentions;
            _messageMentionedRolesSetter(message) = roleMentions;
            _messageMentionedChannelsSetter(message) = channelMentions;
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
            else if (user.Discriminator == "0")
            {
                return user.Username;
            }

            return $"{user.Username}#{user.Discriminator}";
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
