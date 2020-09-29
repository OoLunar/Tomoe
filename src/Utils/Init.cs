using System;
using System.IO;
using Newtonsoft.Json;
using Npgsql.Logging;
using Tomoe.Utils;

namespace Tomoe {
    public class Init {
        private static string logPath = Path.Combine(Program.ProjectRoot, "log/");
        private static string logFile = Path.Combine(logPath, $"{DateTime.Now.ToString("ddd, dd MMM yyyy HH.mm.ss")}.log");
        private static string tokenFile = Path.Combine(Program.ProjectRoot, "res/tokens.jsonc");
        private static string dialogFile = Path.Combine(Program.ProjectRoot, "res/dialog.jsonc");

        public Init() {
            System.Console.WriteLine("[Program] Starting...");
            SetupTokens();
            SetupLogging();
            SetupDatabase();
            SetupDialog();
            Program.Client.Ready += () => { new System.Threading.Thread((System.Threading.ThreadStart) Tomoe.Commands.Tasks.Reminder.StartReminders).Start(); return System.Threading.Tasks.Task.CompletedTask; };
            System.GC.Collect();
        }

        public static void SetupTokens() {
            if (!File.Exists(tokenFile)) {
                FileSystem.CreateFile(tokenFile);
                File.WriteAllText(tokenFile, "{\n\t\"discord_api_token\": \"insert_discord_api_token\", /*Token to have your bot login to Discord.*/\n\t\"log_to_file\": true, /*Log to file instead of console.*/\n\t\"database\": { /*PostgresSQL database. More support will be added in the future.*/\n\t\t\"host\": \"localhost\", /*The IP or hostname that the database is listening on.*/\n\t\t\"port\": 5432, /*The port that the database is listening on.*/\n\t\t\"username\": \"insert_username\", /*The username used to login to the database.*/\n\t\t\"password\": \"insert_password\", /*An alphanumerical password used to login. Must be Postgres compliant.*/\n\t\t\"database\": \"insert_database_name\", /*Database name.*/\n\t\t/* Choose security levels. Options are:\n\t\t * - Require\n\t\t * - Prefer\n\t\t * - Disable\n\t\t * Defaults to Require.*/\n\t\t\"ssl_mode\": \"Require\"\n\t}\n}");
                Console.WriteLine($"[Credentials] '{tokenFile}' was created, which means that the Discord Bot Token and the PostgreSQL information will need to be filled out. All PostgreSQL information should be alphanumeric.");
                System.Environment.Exit(1);
            } else {
                try {
                    Program.Tokens = new JsonSerializer().Deserialize<Tomoe.Tokens>(new JsonTextReader(new StreamReader(System.IO.File.OpenRead(tokenFile))));
                } catch (Newtonsoft.Json.JsonReaderException jsonException) {
                    Console.WriteLine($"[Init] Invalid JSONC on '{tokenFile}'. {jsonException.Message} Exiting.");
                    Environment.Exit(1);
                }
            }
        }

        public static void SetupLogging() {
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Error, true, false);
            if (!FileSystem.CreateFile(logFile)) Console.WriteLine("[Logging] Unable to create the logging file. Everything will be logged to Console.");
            else if (Program.Tokens.LogToFile) {
                Console.WriteLine($"[Logging] 'log_to_file' option in '{tokenFile}' is set to true. Everything will be logged to '{logFile}'");
                StreamWriter sw = new StreamWriter(logFile, true);
                Console.SetError(sw);
                Console.SetOut(sw);
                sw.AutoFlush = true;
            } else {
                Console.WriteLine($"[Logging] 'log_to_file' option in '{tokenFile}' is set to false. Everything will be logged to Console.");
            }
        }

        public static void SetupDialog() {
            if (!File.Exists(dialogFile)) {
                Console.WriteLine($"[Dialog] '{dialogFile}' does not exist. Please download a template from https://github.com/OoLunar/Tomoe/tree/master/res/dialog.jsonc");
                Environment.Exit(1);
            }
            Program.Dialogs = new JsonSerializer().Deserialize<Tomoe.Utils.Dialog.Root>(new JsonTextReader(new StreamReader(System.IO.File.OpenRead(dialogFile))));
        }

        public static void SetupDatabase() {
            Tomoe.Utils.Cache.PreparedStatements.TestConnection();
            Program.PreparedStatements = new Utils.Cache.PreparedStatements();
        }
    }
}