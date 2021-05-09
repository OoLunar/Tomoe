namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    [Group("reminders"), Aliases("remind"), Description("Creates a reminder or todo for later.")]
    public class Reminders : BaseCommandModule
    {
        public Database Database { private get; set; }
        private static List<Reminder> LocalReminders { get; } = new();

        [GroupCommand]
        public async Task Create(CommandContext context, DateTime expiryDate, [RemainingText] string reminderText = Constants.MissingReason)
        {
            Reminder reminder = new();
            reminder.Content = reminderText;
            reminder.ChannelId = context.Channel.Id;
            reminder.Expires = true;
            reminder.ExpiresOn = expiryDate.ToUniversalTime();
            reminder.GuildId = context.Guild.Id;
            reminder.JumpLink = context.Message.JumpLink.ToString();
            reminder.LogId = Database.Reminders.Where(reminder => reminder.UserId == context.User.Id).Count() + 1;
            reminder.MessageId = context.Message.Id;
            reminder.UserId = context.User.Id;

            if (reminder.ExpiresOn <= DateTime.UtcNow.AddMinutes(30))
            {
                LocalReminders.Add(reminder);
            }
            else
            {
                Database.Reminders.Add(reminder);
                await Database.SaveChangesAsync();
            }

            await Program.SendMessage(context, $"Reminder #{reminder.LogId} set! Content: {reminder.Content}");
        }

        [Command("remove")]
        public async Task Remove(CommandContext context, Reminder reminder)
        {
            if (!LocalReminders.Remove(reminder))
            {
                Database.Reminders.Remove(reminder);
                await Database.SaveChangesAsync();
            }
            await Program.SendMessage(context, $"Removed reminder #{reminder.LogId}!");
        }

        [Command("list")]
        public async Task List(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, "Reminders for");
            embedBuilder.Title += $" {context.Member.DisplayName}";
            List<Reminder> reminders = Database.Reminders.AsNoTracking().Where(reminder => reminder.UserId == context.User.Id && reminder.GuildId == context.Guild.Id).ToList();
            int totalPages = reminders.Count / 25;
            List<Page> pages = new();
            for (int i = 0; i < totalPages; i++)
            {
                embedBuilder.ClearFields();
                Page page = new();
                foreach (Reminder reminder in reminders.Take(25))
                {
                    embedBuilder.AddField($"#{reminder.LogId}", Formatter.MaskedUrl(reminder.Content.Truncate(50, "..."), new(reminder.JumpLink), reminder.Content));
                }
                reminders.RemoveRange(0, 25);
                page.Embed = embedBuilder.Build();
                pages.Add(page);
            }

            if (pages.Count == 1)
            {
                await Program.SendMessage(context, null, pages[0].Embed);
            }
            else
            {
                InteractivityExtension interactivity = context.Client.GetInteractivity();
                await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
            }
        }
    }
}
