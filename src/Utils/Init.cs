using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Npgsql.Logging;
using Tomoe.Utils;

namespace Tomoe {
    public class Init {
        private static string LogPath = Path.Combine(Program.ProjectRoot, "log/");
        private static string LogFile = Path.Combine(LogPath, $"{DateTime.Now.ToString("ddd, dd MMM yyyy HH.mm.ss")}.log");
        private static string TokenFile = Path.Combine(Program.ProjectRoot, "res/tokens.xml");
        private static string DialogFile = Path.Combine(Program.ProjectRoot, "res/dialog.xml");

        public Init() {
            SetupTokens();
            SetupLogging();
            SetupDialog();
        }

        public static void SetupTokens() {
            if (!File.Exists(TokenFile)) {
                FileSystem.CreateFile(TokenFile);
                File.WriteAllText(TokenFile, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<config>\n\t<DiscordAPIToken>replace_this_with_bot_token</DiscordAPIToken>\n\t<Postgres host=\"replace_hostname\" port=\"replace_port\" database=\"replace_database_name\" username=\"replace_username\" password=\"replace_password\">\n</Postgres>\n</config>");
                Console.WriteLine($"[Credentials] '{TokenFile}' was created, which means that the Discord Bot Token and the PostgreSQL information will need to be filled out. All PostgreSQL information should be alphanumeric.");
                System.Environment.Exit(1);
            } else {
                try {
                    Program.Tokens.Load(TokenFile);
                } catch (XmlException xmlError) {
                    Console.WriteLine($"[Init] Invalid XML. {xmlError.Message} Exiting.");
                    Environment.Exit(1);
                }
            }
        }

        public static void SetupLogging() {
            NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug, true, false);
            if (!FileSystem.CreateFile(LogFile)) Console.WriteLine("[Logging] Unable to create the logging file. Everything will be logged to Console.");
            else if (bool.Parse(Program.Tokens.DocumentElement.SelectSingleNode("log_to_file").Value) == true) {
                StreamWriter sw = new StreamWriter(LogFile, true);
                Console.SetError(sw);
                Console.SetOut(sw);
                sw.AutoFlush = true;
            } else {
                Console.WriteLine($"[Logging] 'log_to_file' option in '{TokenFile}' is set to false. Everything will be logged to Console.");
            }
        }

        public static void SetupDialog() {
            if (!File.Exists(DialogFile)) {
                Console.WriteLine($"[Dialog] '{DialogFile}' does not exist. Please download a template from https://github.com/OoLunar/Tomoe/tree/master/res.");
                Environment.Exit(1);
            }
            XmlDocument dialogs_available = new XmlDocument();
            dialogs_available.Load(DialogFile);

            foreach (XmlNode node in dialogs_available.DocumentElement) {
                switch (node.Name) {
                    case "mute":
                        foreach (XmlNode section in node) {
                            List<string> phrases = new List<string>();
                            foreach (XmlNode phrase in section.ChildNodes) phrases.Add(phrase.InnerText);
                            Program.Dialogs.Mute.Add(section.Name, phrases.ToArray());
                        }
                        break;
                    default:
                        Console.WriteLine($"[Dialog] Unknown Section '{node.Name}' in '{DialogFile}'... Skipping.");
                        break;
                }
            }
        }
    }

    public class Dialog {
        public Dictionary<string, string[]> Mute = new Dictionary<string, string[]>();
    }
}