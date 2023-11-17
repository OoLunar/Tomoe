using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class CaseCommand
    {
        [Command("case")]
        public static async Task ExecuteAsync(CommandContext context, CaseType caseType, params string[] content)
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
                    _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, null)
                });
            }

            if (content.Length == 1)
            {
                await context.RespondAsync(Formatter.InlineCode(output[0]));
            }
            else
            {
                await context.RespondAsync(Formatter.BlockCode(string.Join('\n', output)));
            }
        }
    }

    public enum CaseType
    {
        Upper,
        Lower,
        Title,
        Snake,
        Pascal,
        Camel
    }
}
