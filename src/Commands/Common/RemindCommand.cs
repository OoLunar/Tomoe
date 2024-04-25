using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("reminders"), TextAlias("remind")]
    public sealed class ReminderCommand
    {
        private static readonly TimeSpanConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeOffsetConverter _dateTimeArgumentConverter = new();

        private readonly DatabaseExpirableManager<ReminderModel, Ulid> _reminderManager;
        public ReminderCommand(DatabaseExpirableManager<ReminderModel, Ulid> reminderManager) => _reminderManager = reminderManager ?? throw new ArgumentNullException(nameof(reminderManager));

        [Command("set"), DefaultGroupCommand]
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
                content = "You wanted me to remind you about... Something. I don't know what though!";
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

        //[Command("list")]
        //public async ValueTask ListAsync(CommandContext context)
        //{
        //    List<ReminderModel> reminders = [];
        //    await foreach (ReminderModel reminderModel in ReminderModel.ListAsync(context.User.Id))
        //    {
        //        reminders.Add(reminderModel);
        //        if (reminders.Count == 5)
        //        {
        //            StringBuilder stringBuilder = new();
        //            foreach (ReminderModel reminder in reminders)
        //            {
        //                stringBuilder.AppendLine($"- {Formatter.Timestamp(reminder.ExpiresAt - DateTimeOffset.UtcNow)}: {reminder.Content}");
        //            }
        //        }
        //    }
        //}
    }
}
