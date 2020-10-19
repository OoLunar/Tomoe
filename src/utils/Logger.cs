using System;
using System.IO;

namespace Tomoe {
    public class Logger {
        public static string LogPath = Path.Combine(Program.ProjectRoot, "log/");
        public static string LogFile = Path.Combine(LogPath, $"{DateTime.Now.ToString("dd MMM yyyy HH.mm.ss")}.log");
        public static bool LogToFile = false;
        public string BranchName;

        public Logger(string branchName) => BranchName = branchName;

        public void Info(string value) => Console.WriteLine($"[{getTime()}]\t[Info]\t{BranchName}: {value}");
        public void Warn(string value) => Console.WriteLine($"[{getTime()}]\t{ConsoleColor.Yellow}[Warning]{ConsoleColor.White}\t{BranchName}: {value}");
        public void Error(string value) => Console.WriteLine($"[{getTime()}]\t{ConsoleColor.Red}[Error]{ConsoleColor.White}\t{BranchName}: {value}");

        private string getTime() => DateTime.Now.ToLocalTime().ToString("ddd, dd MMM yyyy HH':'mm':'ss");

    }
}