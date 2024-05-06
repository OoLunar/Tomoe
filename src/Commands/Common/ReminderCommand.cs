using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Please remind me to remind you to remind me to remind you.
    /// </summary>
    [Command("reminders"), TextAlias("remind")]
    public sealed class ReminderCommand
    {
        private static readonly TimeSpanConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeOffsetConverter _dateTimeArgumentConverter = new();

        private readonly DatabaseExpirableManager<ReminderModel, Ulid> _reminderManager;

        /// <summary>
        /// Creates a new instance of <see cref="ReminderCommand"/>.
        /// </summary>
        /// <param name="reminderManager">Required service for managing reminder data.</param>
        public ReminderCommand(DatabaseExpirableManager<ReminderModel, Ulid> reminderManager) => _reminderManager = reminderManager;

        /// <summary>
        /// Creates a new reminder that will either ping or DM you when it expires.
        /// </summary>
        /// <param name="expiresAt">When you should be notified.</param>
        /// <param name="content">What you want to be reminded about.</param>
        [Command("set"), DefaultGroupCommand, RequirePermissions(DiscordPermissions.AttachFiles, DiscordPermissions.None)]
        public async ValueTask SetAsync(CommandContext context, string expiresAt, [RemainingText] string? content = null)
        {
            DateTimeOffset expires;
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if ((await _timeSpanArgumentConverter.ExecuteAsync(context, expiresAt)).IsDefined(out TimeSpan timeSpan) && timeSpan != default)
            {
                expires = now + timeSpan;
            }
            else if ((await _dateTimeArgumentConverter.ExecuteAsync(context, expiresAt)).IsDefined(out DateTimeOffset dateTime) && dateTime != default)
            {
                expires = dateTime;
            }
            else
            {
                await context.RespondAsync($"Invalid timestamp: `{expiresAt}`");
                return;
            }

            if (expires <= now)
            {
                await context.RespondAsync("I'm sorry, I can't go back in the past to remind you.");
                return;
            }

            ulong messageId = 0;
            if (context is TextCommandContext textContext)
            {
                messageId = textContext.Message.Id;
                content = FormatReminder(textContext.Message, content);
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                content = context is TextCommandContext textCommandContext && textCommandContext.Message.ReferencedMessage is not null
                    ? textCommandContext.Message.ReferencedMessage.Content
                    : "You wanted me to remind you about... Something. I don't know what though!";
            }

            ReminderModel reminderModel = context.Channel is DiscordThreadChannel threadChannel
                ? await ReminderModel.CreateAsync(Ulid.NewUlid(now), context.User.Id, context.Guild?.Id ?? 0, threadChannel.ParentId.GetValueOrDefault(), threadChannel.Id, messageId, expires, ReminderType.OneTime, TimeSpan.Zero, content)
                : await ReminderModel.CreateAsync(Ulid.NewUlid(now), context.User.Id, context.Guild?.Id ?? 0, context.Channel.Id, 0, messageId, expires, ReminderType.OneTime, TimeSpan.Zero, content);

            _reminderManager.AddToCache(reminderModel.Id, reminderModel.ExpiresAt);
            await context.RespondAsync($"Reminder set! I'll ping you {Formatter.Timestamp(expires - now)}.");
        }

        /// <summary>
        /// Lists all reminders you have set.
        /// </summary>
        /// <remarks>
        /// Currently unfinished. Will only list the first 5 reminders.
        /// </remarks>
        [Command("list")]
        public static async ValueTask ListAsync(CommandContext context)
        {
            List<ReminderModel> reminders = [];
            await foreach (ReminderModel reminderModel in ReminderModel.ListAsync(context.User.Id))
            {
                reminders.Add(reminderModel);
                if (reminders.Count == 5)
                {
                    StringBuilder stringBuilder = new();
                    foreach (ReminderModel reminder in reminders)
                    {
                        stringBuilder.AppendLine($"- {Formatter.Timestamp(reminder.ExpiresAt - DateTimeOffset.UtcNow)}: {reminder.Content}");
                    }

                    await context.RespondAsync(stringBuilder.ToString());
                    return;
                }
            }
        }

        private static string FormatReminder(DiscordMessage message, string? content = null)
        {
            StringBuilder stringBuilder = new();

            content ??= message.Content;
            if (!string.IsNullOrWhiteSpace(content))
            {
                stringBuilder.AppendLine(content);
            }

            foreach (DiscordAttachment attachment in message.Attachments)
            {
                stringBuilder.AppendLine(attachment.Url);
            }

            if (message.Poll is not null)
            {
                stringBuilder.Append("Poll: ");
                stringBuilder.AppendLine(message.Poll.Question.Text);
            }

            if (message.ReferencedMessage is not null)
            {
                // We haven't gotten text content, an attachment or a poll... How did this message even send?
                if (stringBuilder.Length == 0)
                {
                    return FormatReminder(message.ReferencedMessage);
                }

                stringBuilder.AppendLine(message.ReferencedMessage.JumpLink.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
