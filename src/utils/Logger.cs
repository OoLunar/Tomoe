using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tomoe.Utils {
    public class Logger : ILogger {

        /// <summary>The area of MCSharp that the logger is Logging.</summary>
        private readonly string _branchName;
        private static Logger _logger = new Logger("Logger");
        public IDisposable BeginScope<TState>(TState state) { throw new NotImplementedException(); }

        /// <summary>
        /// Checks if a 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel) => Tomoe.Program.Config.LogLevel <= logLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            switch (logLevel) {
                case LogLevel.Trace:
                    if (Tomoe.Program.Config.LogLevel <= LogLevel.Trace) Trace(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Debug:
                    if (Tomoe.Program.Config.LogLevel <= LogLevel.Debug) Debug(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Information:
                    if (Tomoe.Program.Config.LogLevel <= LogLevel.Information) Info(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Warning:
                    if (Tomoe.Program.Config.LogLevel <= LogLevel.Warning) Warn(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Error:
                    if (Tomoe.Program.Config.LogLevel <= LogLevel.Error) Error(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Critical:
                    if (Tomoe.Program.Config.LogLevel <= LogLevel.Critical) Critical(formatter(state, exception) ?? exception.Message, false);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Creates a new logger.
        /// </summary>
        /// <example>
        /// <code>
        /// Logger logger = new Logger("Main");
        /// logger.Info("Created Main logger!"); //Output something similar to 
        ///                                      //[Fri, Oct 13 2020 17:32:54] [Info] Main: Created Main logger!
        /// </code>
        /// </example>
        /// <param name="branchName">The area of MCSharp that the logger is Logging.</param>
        public Logger(string branchName) => _branchName = branchName;

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Tomoe.Utils.Logger.Program.Config.LogLevel"> isn't on Trace, nothing will log.
        /// </summary>
        /// <example>
        /// <code>
        /// Logger logger = new Logger("Main");
        /// logger.Trace("Created Main logger!"); //Output something similar to 
        ///                                      //[Fri, Oct 13 2020 17:32:54] [Trace] Main: Created Main logger!
        /// </code>
        /// </example>
        /// <remarks>[Trace] has a blue font color.</remarks>
        /// <param name="value">What to be logged.</param>
        public async Task Trace(string value) {
            if (Program.Config.LogLevel > LogLevel.Trace) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"[Trace]   ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Tomoe.Utils.Logger.Program.Config.LogLevel"> isn't on Debug or Trace, nothing will log.
        /// </summary>
        /// <example>
        /// <code>
        /// Logger logger = new Logger("Main");
        /// logger.Debug("Created Main logger!"); //Output something similar to 
        ///                                       //[Fri, Oct 13 2020 17:32:54] [Debug] Main: Created Main logger!
        /// </code>
        /// </example>
        /// <remarks>[Debug] has a dark grey font color.</remarks>
        /// <param name="value">What to be logged.</param>
        public async Task Debug(string value) {
            if (Program.Config.LogLevel > LogLevel.Debug) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[Debug]   ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Tomoe.Utils.Logger.Program.Config.LogLevel"> isn't on Info or below, nothing will log.
        /// </summary>
        /// <example>
        /// <code>
        /// Logger logger = new Logger("Main");
        /// logger.Info("Created Main logger!"); //Output something similar to 
        ///                                      //[Fri, Oct 13 2020 17:32:54] [Info] Main: Created Main logger!
        /// </code>
        /// </example>
        /// <remarks>[Info] has a green font color.</remarks>
        /// <param name="value">What to be logged.</param>
        /// <param name="exit">Determines if the program exits. Defaults to false.</param>
        public async Task Info(string value, bool exit = false) {
            if (Program.Config.LogLevel > LogLevel.Information) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[Info]    ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (exit) {
                Console.WriteLine("Exiting...");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Tomoe.Utils.Logger.Program.Config.LogLevel"> isn't on Warn or below, nothing will log.
        /// </summary>
        /// <example>
        /// <code>
        /// Logger logger = new Logger("Main");
        /// logger.Warn("Created Main logger!"); //Output something similar to 
        ///                                      //[Fri, Oct 13 2020 17:32:54] [Warn] Main: Created Main logger!
        /// </code>
        /// </example>
        /// <remarks>[Warn] has a yellow font color.</remarks>
        /// <param name="value">What to be logged.</param>
        /// <param name="exit">Determines if the program exits. Defaults to false.</param>
        public async Task Warn(string value, bool exit = false) {
            if (Program.Config.LogLevel > LogLevel.Warning) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[Warning] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (exit) {
                Console.WriteLine("Exiting...");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Tomoe.Utils.Logger.Program.Config.LogLevel"> isn't on Error or below, nothing will log.
        /// </summary>
        /// <example>
        /// <code>
        /// Logger logger = new Logger("Main");
        /// logger.Errpr("Created Main logger!"); //Output something similar to 
        ///                                       //[Fri, Oct 13 2020 17:32:54] [Error] Main: Created Main logger!
        /// </code>
        /// </example>
        /// <remarks>[Error] has a red font color.</remarks>
        /// <param name="value">What to be logged.</param>
        /// <param name="exit">Determines if the program exits. Defaults to false.</param>
        public async Task Error(string value, bool exit = false) {
            if (Program.Config.LogLevel > LogLevel.Error) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[Error]   ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (exit) {
                Console.WriteLine("Exiting...");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Logs all values to console/file. *Will always log.*
        /// </summary>
        /// <example>
        /// <code>
        /// Logger logger = new Logger("Main");
        /// logger.Critical("Created Main logger!"); //Output something similar to 
        ///                                          //[Fri, Oct 13 2020 17:32:54] [Critical] Main: Created Main logger!
        /// </code>
        /// </example>
        /// <remarks>[Critical] has a red font color and white background.</remarks>
        /// <param name="value">What to be logged.</param>
        /// <param name="exit">Determines if the program exits. Defaults to true.</param>
        public async Task Critical(string value, bool exit = true) {
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.Write($"[Critical]");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" {_branchName}");
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (exit) {
                Console.WriteLine("Exiting...");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Gets the time in <a href="https://tools.ietf.org/html/rfc2616#section-14.29">rfc2616, section 14.29 format</a>, slightly tweaked.
        /// </summary>
        /// <returns>string</returns>
        public static string GetTime() => DateTime.Now.ToLocalTime().ToString("ddd, dd MMM yyyy HH':'mm':'ss");
    }

    public class LoggerProvider : ILoggerFactory {
        private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();
        public ILogger CreateLogger(string branchName) => _loggers.GetOrAdd(branchName, name => new Logger(name));
        public void Dispose() => _loggers.Clear();

        public void AddProvider(ILoggerProvider provider) { }
    }
}