using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Sinks.SystemConsole.Themes;

namespace Tomoe.Utils
{
    /// <summary>
    /// Specifies which theme to use for the console logger.
    /// </summary>
    public class LoggerTheme : ConsoleTheme
    {
        /// <inheritdoc />
        public const string AnsiStyleReset = "\x1b[0m";

        /// <summary>
        /// The default theme.
        /// </summary>
        public static AnsiConsoleTheme Lunar { get; } = new AnsiConsoleTheme(
            new Dictionary<ConsoleThemeStyle, string>
            {
                [ConsoleThemeStyle.Text] = AnsiStyleReset,
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
                [ConsoleThemeStyle.LevelFatal] = "\x1b[97;91m",
            }
        );

        private readonly IReadOnlyDictionary<ConsoleThemeStyle, string> _styles;

        /// <summary>
        /// Construct a theme given a set of styles.
        /// </summary>
        /// <param name="styles">Styles to apply within the theme.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="styles"/> is <code>null</code></exception>
        public LoggerTheme(IReadOnlyDictionary<ConsoleThemeStyle, string> styles)
        {
            if (styles is null)
            {
                throw new ArgumentNullException(nameof(styles));
            }

            _styles = styles.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <inheritdoc/>
        public override bool CanBuffer => true;

        /// <inheritdoc/>
        protected override int ResetCharCount { get; } = AnsiStyleReset.Length;

        /// <inheritdoc/>
        public override int Set(TextWriter output, ConsoleThemeStyle style)
        {
            if (_styles.TryGetValue(style, out string? ansiStyle))
            {
                output.Write(ansiStyle);
                return ansiStyle.Length;
            }
            return 0;
        }

        /// <inheritdoc/>
        public override void Reset(TextWriter output) => output.Write(AnsiStyleReset);
    }
}
