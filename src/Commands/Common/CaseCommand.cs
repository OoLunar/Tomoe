using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Mutates text to different cases.
    /// </summary>
    public sealed class CaseCommand
    {
        /// <summary>
        /// Mutates the provided text to the grammatical case.
        /// </summary>
        /// <param name="caseType">The case to modify the text to.</param>
        /// <param name="content">The text that will be modified.</param>
        [Command("case")]
        public static ValueTask ExecuteAsync(CommandContext context, CaseType caseType, params string[] content)
        {
            List<string> output = [];
            foreach (string line in content)
            {
                output.Add(caseType switch
                {
                    CaseType.Upper => line.Trim().ToUpper(CultureInfo.InvariantCulture),
                    CaseType.Lower => line.Trim().ToLower(CultureInfo.InvariantCulture),
                    CaseType.Title => line.Trim().Titleize(),
                    CaseType.Snake => line.Trim().Underscore(),
                    CaseType.Pascal => line.Trim().Pascalize(),
                    CaseType.Camel => line.Trim().Camelize(),
                    CaseType.Kebab => line.Trim().Kebaberize(),
                    _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, null)
                });
            }

            return context.RespondAsync(content.Length == 1
                ? Formatter.InlineCode(output[0])
                : Formatter.BlockCode(string.Join('\n', output))
            );
        }
    }

    /// <summary>
    /// The different cases that text can be mutated to.
    /// </summary>
    public enum CaseType
    {
        /// <summary>
        /// Converts the text to all UPPERCASE.
        /// </summary>
        Upper,

        /// <summary>
        /// Converts the text to all lowercase.
        /// </summary>
        Lower,

        /// <summary>
        /// Capitalizes the First Letter of Each Word Where Appropriate.
        /// </summary>
        Title,

        /// <summary>
        /// Converts the text to PascalCase.
        /// </summary>
        Pascal,

        /// <summary>
        /// Converts the text to camelCase.
        /// </summary>
        Camel,

        /// <summary>
        /// Converts the text to snake_case.
        /// </summary>
        Snake,

        /// <summary>
        /// Converts the text to kebab-case.
        /// </summary>
        Kebab
    }
}
