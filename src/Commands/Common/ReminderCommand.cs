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
            else if (string.IsNullOrWhiteSpace(content))
            {
                content = context is TextCommandContext textCommandContext && textCommandContext.Message.ReferencedMessage is not null
                    ? textCommandContext.Message.ReferencedMessage.Content
                    : "You wanted me to remind you about... Something. I don't know what though!";
            }

            ulong messageId = 0;
            if (context is TextCommandContext textContext)
            {
                messageId = textContext.Message.Id;
            }

            ReminderModel reminderModel = await ReminderModel.CreateAsync(Ulid.NewUlid(), context.User.Id, context.Guild?.Id ?? 0, context.Channel.Id, messageId, expires, ReminderType.OneTime, TimeSpan.Zero, content);
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
    }
}
