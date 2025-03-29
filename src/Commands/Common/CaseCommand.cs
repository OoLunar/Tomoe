using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        public static async ValueTask ExecuteAsync(CommandContext context, CaseType caseType, params string[] content)
        {
            CultureInfo userCulture = await context.GetCultureAsync();
            await context.RespondAsync(string.Join('\n', caseType switch
            {
                CaseType.Upper => content.Select(str => str.Trim().ToUpper(userCulture)),
                CaseType.Lower => content.Select(str => str.Trim().ToLower(userCulture)),
                CaseType.Title => content.Select(str => str.Trim().Titleize()),
                CaseType.Snake => content.Select(str => str.Trim().Underscore()),
                CaseType.Pascal => content.Select(str => str.Trim().Pascalize()),
                CaseType.Camel => content.Select(str => str.Trim().Camelize()),
                CaseType.Kebab => content.Select(str => str.Trim().Kebaberize()),
                _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, null)
            }));
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
