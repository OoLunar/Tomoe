using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Npgsql.Logging;
using Tomoe.Utils;

namespace Tomoe {
    public class Init {
        private static string logPath = Path.Combine(Program.ProjectRoot, "log/");
        private static string logFile = Path.Combine(logPath, $"{DateTime.Now.ToString("ddd, dd MMM yyyy HH.mm.ss")}.log");
        private static string tokenFile = Path.Combine(Program.ProjectRoot, "res/tokens.xml");
        private static string dialogFile = Path.Combine(Program.ProjectRoot, "res/dialog.jsonc");

        public Init() {
            System.Console.WriteLine("[Program] Starting...");
            SetupTokens();
            SetupLogging();
            SetupDatabase();
            SetupDialog();
            Program.Client.Ready += () => { new System.Threading.Thread((System.Threading.ThreadStart) Tomoe.Commands.Reminder.StartReminders).Start(); return System.Threading.Tasks.Task.CompletedTask; };
            System.GC.Collect();
        }

        public static void SetupTokens() {
            if (!File.Exists(tokenFile)) {
                FileSystem.CreateFile(tokenFile);
                File.WriteAllText(tokenFile, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<config>\n\t<DiscordAPIToken>replace_this_with_bot_token</DiscordAPIToken>\n\t<Postgres host=\"replace_hostname\" port=\"replace_port\" database=\"replace_database_name\" username=\"replace_username\" password=\"replace_password\">\n</Postgres>\n</config>");
                Console.WriteLine($"[Credentials] '{tokenFile}' was created, which means that the Discord Bot Token and the PostgreSQL information will need to be filled out. All PostgreSQL information should be alphanumeric.");
                System.Environment.Exit(1);
            } else {
                try {
                    Program.Tokens.Load(tokenFile);
                } catch (XmlException xmlError) {
                    Console.WriteLine($"[Init] Invalid XML on '{tokenFile}'. {xmlError.Message} Exiting.");
                    Environment.Exit(1);
                }
            }
        }

        public static void SetupLogging() {
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Warn, true, false);
            if (!FileSystem.CreateFile(logFile)) Console.WriteLine("[Logging] Unable to create the logging file. Everything will be logged to Console.");
            else if (bool.Parse(Program.Tokens.DocumentElement.SelectSingleNode("log_to_file").InnerText) == true) {
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
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            Program.Dialogs = new JsonSerializer().Deserialize<Dialog>(new JsonTextReader(new StreamReader(System.IO.File.OpenRead("res/dialog.jsonc"))));
        }

        public static void SetupDatabase() {
            Tomoe.Utils.Cache.PreparedStatements.TestConnection();
            Program.PreparedStatements = new Utils.Cache.PreparedStatements();
        }
    }
}