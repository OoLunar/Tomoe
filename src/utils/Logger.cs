using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql.Logging;

namespace Tomoe.Utils {
    public class Logger : ILogger {

        /// <summary>What to prefix the log content with.</summary>
        private readonly string _branchName;
        /// <summary>A per branch log level. Defaults to the config's log level.</summary>
        private readonly LogLevel _branchLogLevel;
        /// <summary>The logger's logger. Ironic.</summary>
        private static Logger _logger = new Logger("Logger");

        /// <summary>The log file.</summary>
        private static FileStream _logFile = new FileStream(Path.Join(FileSystem.ProjectRoot, $"log/{GetTime()}.log"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        /// <summary>Unknown what this does. TODO: Implement this correctly.</summary>
        public IDisposable BeginScope<TState>(TState state) { throw new NotImplementedException(); }

        /// <summary>
        /// Tests if a log level is enabled. Compares the <paramref name="logLevel">logLevel</paramref> with <see cref="Config.LogLevel">Config.LogLevel</see>.
        /// </summary>
        /// <param name="logLevel">What level to test if is activated.</param>
        /// <returns>true if <paramref name="logLevel">logLevel</paramref> is enabled, otherwise false.</returns>
        public bool IsEnabled(LogLevel logLevel) => Config.LogLevel <= logLevel;

        /// <summary>
        /// Logs stuff to console.
        /// </summary>
        /// <param name="logLevel">What level you're logging the content at.</param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        /// <typeparam name="TState"></typeparam>
        /// <remarks>While this isn't depreciated, it is advised that you use <see cref="Tomoe.Utils.Logger.Log(LogLevel, string)">Log(LogLevel, string)</see> instead, as this function isn't reliably formatting the formatter correctly.</remarks>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            switch (logLevel) {
                case LogLevel.Trace:
                    Trace(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Debug:
                    Debug(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Information:
                    Info(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Warning:
                    Warn(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Error:
                    Error(formatter(state, exception) ?? exception.Message);
                    break;
                case LogLevel.Critical:
                    Critical(formatter(state, exception) ?? exception.Message, false);
                    break;
                default:
                    break;
            }
        }

        public void Log(LogLevel logLevel, string content) {
            switch (logLevel) {
                case LogLevel.Trace:
                    Trace(content);
                    break;
                case LogLevel.Debug:
                    Debug(content);
                    break;
                case LogLevel.Information:
                    Info(content);
                    break;
                case LogLevel.Warning:
                    Warn(content);
                    break;
                case LogLevel.Error:
                    Error(content);
                    break;
                case LogLevel.Critical:
                    Critical(content, false);
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
        public Logger(string branchName) {
            _branchName = branchName;
            _branchLogLevel = Config.LogLevel;
        }

        public Logger(string branchName, LogLevel branchLogLevel) {
            _branchName = branchName;
            _branchLogLevel = branchLogLevel;
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Config.LogLevel"> isn't on Trace, nothing will log.
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
            if (Config.LogLevel > LogLevel.Trace || _branchLogLevel > LogLevel.Trace) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"[Trace] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (Config.LogToFile && _logFile != null) {
                await _logFile.WriteAsync(Encoding.UTF8.GetBytes($"[{currentTime}] [Trace] {_branchName}: {value}{Environment.NewLine}"));
                await _logFile.FlushAsync();
            }
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Config.LogLevel"> isn't on Debug or Trace, nothing will log.
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
            if (Config.LogLevel > LogLevel.Debug || _branchLogLevel > LogLevel.Debug) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[Debug] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (Config.LogToFile && _logFile != null) {
                await _logFile.WriteAsync(Encoding.UTF8.GetBytes($"[{currentTime}] [Debug] {_branchName}: {value}{Environment.NewLine}"));
                await _logFile.FlushAsync();
            }
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Config.LogLevel"> isn't on Info or below, nothing will log.
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
            if (Config.LogLevel > LogLevel.Information || _branchLogLevel > LogLevel.Information) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[Info]  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (Config.LogToFile && _logFile != null) {
                await _logFile.WriteAsync(Encoding.UTF8.GetBytes($"[{currentTime}] [Info]  {_branchName}: {value}{Environment.NewLine}"));
                await _logFile.FlushAsync();
            }
            if (exit) {
                _logFile.Dispose();
                Console.WriteLine("Exiting...");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Config.LogLevel"> isn't on Warn or below, nothing will log.
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
            if (Config.LogLevel > LogLevel.Warning || _branchLogLevel > LogLevel.Warning) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[Warn]  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (Config.LogToFile && _logFile != null) {
                await _logFile.WriteAsync(Encoding.UTF8.GetBytes($"[{currentTime}] [Warn]  {_branchName}: {value}{Environment.NewLine}"));
                await _logFile.FlushAsync();
            }
            if (exit) {
                _logFile.Dispose();
                Console.WriteLine("Exiting...");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Logs all values to console/file. If the <see cref="Config.LogLevel"> isn't on Error or below, nothing will log.
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
            if (Config.LogLevel > LogLevel.Error || _branchLogLevel > LogLevel.Error) return;
            string currentTime = GetTime();
            Console.ResetColor();
            Console.Write($"[{currentTime}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[Error] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (Config.LogToFile && _logFile != null) {
                await _logFile.WriteAsync(Encoding.UTF8.GetBytes($"[{currentTime}] [Error] {_branchName}: {value}{Environment.NewLine}"));
                await _logFile.FlushAsync();
            }
            if (exit) {
                _logFile.Dispose();
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
            Console.Write($"[Crit]  ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(_branchName);
            Console.ResetColor();
            Console.WriteLine($": {value}");
            if (Config.LogToFile && _logFile != null) {
                await _logFile.WriteAsync(Encoding.UTF8.GetBytes($"[{currentTime}] [Crit]  {_branchName}: {value}{Environment.NewLine}"));
                await _logFile.FlushAsync();
            }
            if (exit) {
                _logFile.Dispose();
                Console.WriteLine("Exiting...");
                Environment.Exit(1);
            }
        }

        /// <summary>Gets the time in <code>yyyy-MM-dd HH:mm:ss</code> following rfc3339 format, slightly tweaked (see removed 'T'). See https://tools.ietf.org/html/rfc3339#section-5.6.</summary>
        /// <returns>string</returns>
        public static string GetTime() => DateTime.Now.ToLocalTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
    }

    public class LoggerProvider : ILoggerFactory {
        private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();
        public ILogger CreateLogger(string branchName) {
            if (branchName.ToLower().StartsWith("dsharpplus")) return _loggers.GetOrAdd(branchName, name => new Logger(name, Config.DiscordLogLevel));
            else return _loggers.GetOrAdd(branchName, name => new Logger(name));
        }

        public void Dispose() => _loggers.Clear();
        public void AddProvider(ILoggerProvider provider) { }
    }

    class NLogLoggingProvider : INpgsqlLoggingProvider {
        public NpgsqlLogger CreateLogger(string name) => new NpgsqlToLogger(name);
    }

    class NpgsqlToLogger : NpgsqlLogger {
        private static Logger _logger;
        internal NpgsqlToLogger(string name) => _logger = new Logger(name, Config.NpgsqlLogLevel);
        public override bool IsEnabled(NpgsqlLogLevel level) => _logger.IsEnabled(ToLogLevel(level));
        public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception? exception) {
            _logger.Log(ToLogLevel(level), $"{msg}{(exception == null ? null : '\n' + exception.ToString())}");
        }

        static LogLevel ToLogLevel(NpgsqlLogLevel level) {
            switch (level) {
                case NpgsqlLogLevel.Trace:
                    return LogLevel.Trace;
                case NpgsqlLogLevel.Debug:
                    return LogLevel.Debug;
                case NpgsqlLogLevel.Info:
                    return LogLevel.Information;
                case NpgsqlLogLevel.Warn:
                    return LogLevel.Warning;
                case NpgsqlLogLevel.Error:
                    return LogLevel.Error;
                case NpgsqlLogLevel.Fatal:
                    return LogLevel.Critical;
                default:
                    return LogLevel.Debug;
            }
        }
    }
}