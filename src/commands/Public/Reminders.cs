using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Npgsql;
using Tomoe.Utils;

namespace Tomoe.Commands.Public {
    [Group("remind")]
    [Aliases("reminders")]
    [Description("Creates a reminder to go off at the specified time.")]
    public class Reminders : BaseCommandModule {
        private static Logger _logger = new Logger("Commands/Public/Tasks");
        [GroupCommand]
        public async Task Create(CommandContext context, TimeSpan setOff, [RemainingText] string content) {
            Program.Database.Tasks.Create(Database.Interfaces.TaskType.Reminder, context.Guild.Id, context.Channel.Id, context.Message.Id, context.User.Id, DateTime.Now + setOff, DateTime.Now, content);
            Program.SendMessage(context, $"Set off at {DateTime.Now.Add(setOff).ToString("MMM dd', 'HHH':'mm':'ss")}: ```\n{content}```", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
        }

        [GroupCommand]
        public async Task ListByGroup(CommandContext context) => await List(context);

        [Command("list")]
        [Description("Lists what reminders are set.")]
        public async Task List(CommandContext context) {
            Tomoe.Database.Interfaces.Task[] tasks = Program.Database.Tasks.SelectAllReminders(context.User.Id);
            List<string> reminders = new List<string>();
            if (tasks == null) reminders.Add("No reminders are set!");
            else {
                _logger.Trace(tasks.Length.ToString());
                foreach (Tomoe.Database.Interfaces.Task task in tasks) {
                    _logger.Trace($"Id #{task.TaskId}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss")}: {task.Content}");
                    reminders.Add($"Id #{task.TaskId}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss")}: {task.Content}");
                }
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithAuthor(context.Guild.Name, context.Guild.IconUrl ?? context.User.DefaultAvatarUrl, context.Guild.IconUrl ?? context.User.DefaultAvatarUrl);
            embedBuilder.WithTitle($"All reminders for {context.Member.GetCommonName()}");
            var interactivity = context.Client.GetInteractivity();
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join("\n", reminders.ToArray()), SplitType.Character, embedBuilder), timeoutoverride : TimeSpan.FromMinutes(2));
        }

        [Command("remove")]
        [Description("Removes a reminder.")]
        public async Task Remove(CommandContext context, int taskId) {
            _logger.Trace("Executing remove");
            Tomoe.Database.Interfaces.Task? task = Program.Database.Tasks.Select(context.User.Id, Database.Interfaces.TaskType.Reminder, taskId);
            if (!task.HasValue) Program.SendMessage(context, $"Reminder #{taskId} does not exist!");
            else {
                Program.Database.Tasks.Remove(taskId);
                Program.SendMessage(context, $"Reminder #{taskId} was removed!");
            }
        }

        [Command("remove")]
        public async Task Remove(CommandContext context, string taskId) {
            try {
                Remove(context, int.Parse(taskId.Remove(0, 1)));
            } catch (FormatException) {
                Program.SendMessage(context, $"\"{taskId}\" is not an id.");
            }
        }

        public static void StartRoutine() {
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += async(object sender, ElapsedEventArgs e) => {
                Tomoe.Database.Interfaces.Task[] tasks;
                try {
                    tasks = Program.Database.Tasks.SelectAllTasks();
                } catch (NpgsqlOperationInProgressException) {
                    return;
                }
                if (tasks == null) return;
                foreach (Tomoe.Database.Interfaces.Task task in tasks) {
                    if (task.SetOff.CompareTo(DateTime.Now) < 0) {
                        MakeShiftContext context = new MakeShiftContext(task.GuildId, task.ChannelId, task.MessageId, task.UserId);
                        switch (task.TaskType) {
                            case Tomoe.Database.Interfaces.TaskType.Reminder:
                                Program.SendMessage(context, $"Set at {task.SetAt.ToString("MMM dd', 'HHH':'mm")}: {task.Content}", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
                                Program.Database.Tasks.Remove(task.TaskId);
                                break;
                        }
                    }
                }
            };
            timer.Start();
        }
    }
}