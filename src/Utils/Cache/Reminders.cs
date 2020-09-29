using System;
using System.Collections.Generic;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class Tasks {
        public List<Tomoe.Commands.Tasks.Reminder.Action> TaskType = new List<Commands.Tasks.Reminder.Action>();
        public List<ulong> GuildID = new List<ulong>();
        public List<ulong> ChannelID = new List<ulong>();
        public List<ulong> UserID = new List<ulong>();
        public List<DateTime> SetOff = new List<DateTime>();
        public List<DateTime> SetAt = new List<DateTime>();
        public List<string> Content = new List<string>();

        public Tasks() { }

        public static Tasks? GetTasks(Tomoe.Commands.Tasks.Reminder.Action taskType, ulong userID) {
            PreparedStatements.Query getTask = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetTask];
            getTask.Parameters["taskType"].Value = (short) taskType;
            getTask.Parameters["userID"].Value = (long) userID;
            NpgsqlDataReader dataReader = getTask.Command.ExecuteReader();
            Tasks task = new Tasks();
            while (dataReader.Read()) {
                task.TaskType.Add((Tomoe.Commands.Tasks.Reminder.Action) int.Parse(dataReader[0].ToString()));
                task.GuildID.Add(ulong.Parse(dataReader[1].ToString()));
                task.ChannelID.Add(ulong.Parse(dataReader[2].ToString()));
                task.UserID.Add(ulong.Parse(dataReader[3].ToString()));
                task.SetOff.Add(DateTime.Parse(dataReader[4].ToString()));
                task.SetAt.Add(DateTime.Parse(dataReader[5].ToString()));
                task.Content.Add((string) dataReader[6]);
            }
            dataReader.Close();
            return task;
        }

        public static Tasks? GetTasks() {
            PreparedStatements.Query getTask = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetAllTasks];
            NpgsqlDataReader dataReader = getTask.Command.ExecuteReader();
            Tasks allTasks = new Tasks();
            while (dataReader.Read()) {
                allTasks.TaskType.Add((Tomoe.Commands.Tasks.Reminder.Action) dataReader.GetInt32(0));
                allTasks.GuildID.Add(ulong.Parse(dataReader[1].ToString()));
                allTasks.ChannelID.Add(ulong.Parse(dataReader[2].ToString()));
                allTasks.UserID.Add(ulong.Parse(dataReader[3].ToString()));
                allTasks.SetOff.Add(dataReader.GetDateTime(4));
                allTasks.SetAt.Add(dataReader.GetDateTime(5));
                allTasks.Content.Add(dataReader[6].ToString());
            }
            dataReader.Close();
            return allTasks;
        }

        public static void AddTask(Tomoe.Commands.Tasks.Reminder.Action taskType, ulong guildID, ulong channelID, ulong userID, DateTime setOff, DateTime setAt, string content) {
            PreparedStatements.Query addTask = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetTask];
            addTask.Parameters["taskType"].Value = (short) taskType;
            addTask.Parameters["guildID"].Value = (long) guildID;
            addTask.Parameters["channelID"].Value = (long) channelID;
            addTask.Parameters["userID"].Value = (long) userID;
            addTask.Parameters["setOff"].Value = setOff;
            addTask.Parameters["setAt"].Value = setAt;
            addTask.Parameters["content"].Value = content;
            addTask.Command.ExecuteNonQuery();
        }

        public static void RemoveTask(Tomoe.Commands.Tasks.Reminder.Action taskType, ulong userID, DateTime setAt, DateTime setOff) {
            PreparedStatements.Query removeTask = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.RemoveTask];
            removeTask.Parameters["taskType"].Value = (short) taskType;
            removeTask.Parameters["userID"].Value = (long) userID;
            removeTask.Parameters["setOff"].Value = setOff;
            removeTask.Parameters["setAt"].Value = setAt;
            removeTask.Command.ExecuteNonQuery();
        }
    }
}