using System;
using System.Timers;
using Discord;

// Purposely run code async, without await.
#pragma warning disable CS4014

namespace Tomoe.Commands {
    public class Reminder {
        public static void StartReminders() {
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += async(object sender, ElapsedEventArgs e) => {
                Tomoe.Utils.Cache.Tasks tasks = null;
                try {
                    tasks = Tomoe.Utils.Cache.Tasks.GetTasks();
                }
                // Catch ongoing operations and skips the current operation.
                catch (Npgsql.NpgsqlOperationInProgressException) { return; }

                for (int i = 0; i < tasks.TaskType.Count; i++) {
                    if (tasks.SetOff[i] < DateTime.Now) {
                        switch (tasks.TaskType[i].ToString()) {
                            case "UnBan":
                                Tomoe.Commands.Moderation.UnBan.ByProgram(tasks.GuildID[i], tasks.UserID[i], tasks.Content[i]);
                                Tomoe.Utils.Cache.Tasks.RemoveTask(tasks.TaskType[i], tasks.UserID[i], tasks.SetAt[i], tasks.SetOff[i]);
                                break;
                                /*
                                case "UnMute":
                                    Tomoe.Commands.Moderation.UnMute.ByProgram(tasks.GuildID[i], tasks.UserID[i], tasks.Content[i]);
                                    break;
                                */
                            case "Reminder":
                                Discord.WebSocket.ISocketMessageChannel reminderChannel = (Discord.WebSocket.ISocketMessageChannel) Program.Client.GetChannel(tasks.ChannelID[i]);
                                Discord.WebSocket.SocketGuildUser user = (Discord.WebSocket.SocketGuildUser) Program.Client.GetGuild(tasks.GuildID[i]).GetUser(tasks.UserID[i]);
                                if (user != null) reminderChannel.SendMessageAsync($"{user.Mention}: **On {tasks.SetAt[i].ToString()}:** {tasks.Content[i].Replace("\\n", "\n")}");
                                else {
                                    IDMChannel userDM = await Program.Client.GetDMChannelAsync(tasks.UserID[i]);
                                    userDM.SendMessageAsync($"{user.Mention}: **On {tasks.SetAt[i].ToString()} in the guild {Program.Client.GetGuild(tasks.GuildID[i]).Name}:** {tasks.Content[i].Replace("\\n", "\n")}");
                                }
                                Tomoe.Utils.Cache.Tasks.RemoveTask(tasks.TaskType[i], tasks.UserID[i], tasks.SetAt[i], tasks.SetOff[i]);
                                Console.WriteLine($"Removed: {tasks.TaskType[i]}, {tasks.UserID[i]}, {tasks.SetAt[i]}, {tasks.SetOff[i]}");
                                break;
                        };
                    }
                }
            };
            timer.Start();
        }

        public enum Action {
            UnBan,
            UnMute,
            UnNoMeme,
            Reminder
        }

    }
}