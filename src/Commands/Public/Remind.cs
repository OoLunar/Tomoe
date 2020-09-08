using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Public {
    public class Remind : InteractiveBase {

        /// <summary>
        /// Sends a Discord message, then edits the message with how long it took to send in milliseconds.
        /// <code>
        /// >>ping
        /// </code>
        /// </summary>
        [Command("remind", RunMode = RunMode.Async)]
        public async Task remindWithContext(TimeSpan when, [Remainder] string message) {
            Tomoe.Utils.Cache.Tasks.AddTask(Reminder.Action.Reminder, Context.Guild.Id, Context.Channel.Id, Context.User.Id, DateTime.Now + when, DateTime.Now, $"{message.Replace("\n", "\\n")}\\nContext: <{Context.Message.GetJumpUrl()}>");
            await Context.Message.AddReactionAsync(new Discord.Emoji("👍"));
        }

        [Command("remind", RunMode = RunMode.Async)]
        public async Task remindWithoutContext(TimeSpan when) {
            Tomoe.Utils.Cache.Tasks.AddTask(Reminder.Action.Reminder, Context.Guild.Id, Context.Channel.Id, Context.User.Id, DateTime.Now + when, DateTime.Now, $"You wanted to be reminded, but you didn't say why.\\nContext: <{Context.Message.GetJumpUrl()}>");
            await Context.Message.AddReactionAsync(new Discord.Emoji("👍"));
        }

        [Command("reminders", RunMode = RunMode.Async)]
        public async Task showReminders() {
            Tomoe.Utils.Cache.Tasks reminders = Tomoe.Utils.Cache.Tasks.GetTasks(Reminder.Action.Reminder, Context.User.Id);
            if (reminders != null) {
                List<string> embedList = new List<string>(10);
                string embed = ">>> ";
                for (int i = 0; i < reminders.TaskType.Count; i++) {
                    string reminderDialog = $"**On {reminders.SetAt[i].ToString()}:** {reminders.Content[i].Replace("\\n", ". ").Replace("..", ".")}\n";
                    if ((embed + reminderDialog).Length <= 2000) embed += reminderDialog;
                    else {
                        if (embedList.Count <= 9) {
                            embedList.Add(embed);
                            embed = ">>> ";
                        } else {
                            await PagedReplyAsync(embedList);
                            await ReplyAsync($"{Tomoe.Utils.ExtensionMethods.GetCommonName(Context.User)}: You have more reminders that cannot be shown.");
                            return;
                        }
                    }
                }
                if (embed.Trim() == ">>>" && embedList.Count == 0)
                    await ReplyAsync("You have no reminders.");
                else if (embed.Trim() != ">>>" && embedList.Count == 0)
                    await ReplyAsync(embed);
            } else await ReplyAsync($"{Tomoe.Utils.ExtensionMethods.GetCommonName(Context.User)}: You have no reminders.");
        }
    }
}