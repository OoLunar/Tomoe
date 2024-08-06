using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
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
        public static async ValueTask ExecuteAsync(CommandContext context, CaseType caseType, [RemainingText] string content) => await context.RespondAsync(caseType switch
        {
            CaseType.Upper => content.Trim().ToUpper(CultureInfo.InvariantCulture),
            CaseType.Lower => content.Trim().ToLower(CultureInfo.InvariantCulture),
            CaseType.Title => content.Trim().Titleize(),
            CaseType.Snake => content.Trim().Underscore(),
            CaseType.Pascal => content.Trim().Pascalize(),
            CaseType.Camel => content.Trim().Camelize(),
            CaseType.Kebab => content.Trim().Kebaberize(),
            _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, null)
        });
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
