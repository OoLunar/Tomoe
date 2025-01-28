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
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

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
        [Command("set"), DefaultGroupCommand, RequirePermissions([DiscordPermission.AttachFiles], [])]
        public async ValueTask SetAsync(CommandContext context, string expiresAt, [RemainingText] string? content = null)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            TextConverterContext converterContext = new()
            {
                Channel = context.Channel,
                Command = context.Command,
                Extension = context.Extension,
                RawArguments = expiresAt,
                Message = null!,
                ServiceScope = context.ServiceProvider.CreateAsyncScope(),
                Splicer = context.Extension.GetProcessor<TextCommandProcessor>().Configuration.TextArgumentSplicer,
                User = context.User
            };

            converterContext.NextArgument();

            DateTimeOffset expires;
            if ((await _timeSpanArgumentConverter.ConvertAsync(converterContext)).IsDefined(out TimeSpan timeSpan) && timeSpan != default)
            {
                expires = now + timeSpan;
            }
            else if ((await _dateTimeArgumentConverter.ConvertAsync(converterContext)).IsDefined(out DateTimeOffset dateTimeOffset) && dateTimeOffset != default)
            {
                TimeSpan offset = (await context.GetTimeZoneAsync()).BaseUtcOffset;
                expires = dateTimeOffset.ToOffset(offset).Subtract(offset);
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
                ? await ReminderModel.CreateAsync(Ulid.NewUlid(now), context.User.Id, context.Guild?.Id ?? 0, threadChannel.ParentId.GetValueOrDefault(), threadChannel.Id, messageId, expires, TimeSpan.Zero, content)
                : await ReminderModel.CreateAsync(Ulid.NewUlid(now), context.User.Id, context.Guild?.Id ?? 0, context.Channel.Id, 0, messageId, expires, TimeSpan.Zero, content);

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
            List<Page> pages = [];
            await foreach (ReminderModel reminderModel in ReminderModel.ListAsync(context.User.Id))
            {
                DiscordEmbedBuilder embedBuilder = new()
                {
                    Color = new DiscordColor(0x6b73db),
                    Description = reminderModel.Content
                };

                embedBuilder.AddField("Created At", Formatter.Timestamp(reminderModel.Id.Time, TimestampFormat.LongDateTime));
                embedBuilder.AddField("Expires At", Formatter.Timestamp(reminderModel.ExpiresAt - DateTimeOffset.UtcNow));
                pages.Add(new Page(new DiscordMessageBuilder().AddEmbed(embedBuilder)));
            }

            if (pages.Count == 0)
            {
                await context.RespondAsync("You don't have any reminders set.");
                return;
            }

            await context.PaginateAsync(pages);
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
