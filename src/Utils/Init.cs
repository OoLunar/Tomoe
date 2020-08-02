using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Npgsql.Logging;
using Tomoe.Utils;

namespace Tomoe {
    public class Init {
        private static string logPath = Path.Combine(Program.ProjectRoot, "log/");
        private static string logFile = Path.Combine(logPath, $"{DateTime.Now.ToString("ddd, dd MMM yyyy HH.mm.ss")}.log");
        private static string tokenFile = Path.Combine(Program.ProjectRoot, "res/tokens.xml");
        private static string dialogFile = Path.Combine(Program.ProjectRoot, "res/dialog.xml");

        public Init() {
            System.Console.WriteLine("[Program] Starting...");
            SetupTokens();
            SetupLogging();
            SetupDatabase();
            SetupDialog();
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
                Console.WriteLine($"[Dialog] '{dialogFile}' does not exist. Please download a template from https://github.com/OoLunar/Tomoe/tree/master/res.");
                Environment.Exit(1);
            }
            XmlDocument dialogs_available = new XmlDocument();
            try {
                dialogs_available.Load(dialogFile);
            } catch (XmlException xmlError) {
                Console.WriteLine($"[Init] Invalid XML on '{dialogFile}'. {xmlError.Message} Exiting.");
                Environment.Exit(1);
            }

            foreach (XmlNode node in dialogs_available.DocumentElement) {
                if (Array.IndexOf(Dialog.loadCategories, node.Name) >= 0) {
                    foreach (XmlNode section in node) {
                        List<string> phrases = new List<string>();
                        foreach (XmlNode phrase in section.ChildNodes) phrases.Add(phrase.InnerText);
                        Program.Dialogs.Add(node.Name, section.Name, phrases.ToArray());
                    }
                } else {
                    Console.WriteLine($"[Dialog] Unknown Section '{node.Name}' in '{dialogFile}'... Skipping.");
                }
            }
        }

        public static void SetupDatabase() {
            Tomoe.Utils.Cache.PreparedStatements.TestConnection();
            Program.PreparedStatements = new Utils.Cache.PreparedStatements();
        }
    }
}

public class Dialog {
    public static string[] loadCategories = { "mute", "kick", "guild_setup", "mute_setup" };
    public Dictionary<string, string[]> Mute = new Dictionary<string, string[]>();
    public Dictionary<string, string[]> Kick = new Dictionary<string, string[]>();
    public Dictionary<string, string[]> MuteSetup = new Dictionary<string, string[]>();
    public Dictionary<string, string[]> GuildSetup = new Dictionary<string, string[]>();

    public void Add(string categoryName, string sectionName, string[] phrases) {
        if (categoryName == "mute") Mute.Add(sectionName, phrases);
        else if (categoryName == "kick") Kick.Add(sectionName, phrases);
        else if (categoryName == "guild_setup") GuildSetup.Add(sectionName, phrases);
        else if (categoryName == "mute_setup") MuteSetup.Add(sectionName, phrases);
        else Console.WriteLine("[Dialog] Attempted to add category that doesn't exist. Ignoring.");
    }
}