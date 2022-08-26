using System.Collections.Generic;
using Serilog.Sinks.SystemConsole.Themes;

namespace OoLunar.Tomoe.Utilities
{
    public static class LoggerTheme
    {
        public static AnsiConsoleTheme Lunar => new(new Dictionary<ConsoleThemeStyle, string>
        {
            [ConsoleThemeStyle.Text] = "\x1b[0m",
            [ConsoleThemeStyle.SecondaryText] = "\x1b[90m",
            [ConsoleThemeStyle.TertiaryText] = "\x1b[90m",
            [ConsoleThemeStyle.Invalid] = "\x1b[31m",
            [ConsoleThemeStyle.Null] = "\x1b[95m",
            [ConsoleThemeStyle.Name] = "\x1b[93m",
            [ConsoleThemeStyle.String] = "\x1b[96m",
            [ConsoleThemeStyle.Number] = "\x1b[95m",
            [ConsoleThemeStyle.Boolean] = "\x1b[95m",
            [ConsoleThemeStyle.Scalar] = "\x1b[95m",
            [ConsoleThemeStyle.LevelVerbose] = "\x1b[34m",
            [ConsoleThemeStyle.LevelDebug] = "\x1b[90m",
            [ConsoleThemeStyle.LevelInformation] = "\x1b[36m",
            [ConsoleThemeStyle.LevelWarning] = "\x1b[33m",
            [ConsoleThemeStyle.LevelError] = "\x1b[31m",
            [ConsoleThemeStyle.LevelFatal] = "\x1b[97;91m"
        });
    }
}
