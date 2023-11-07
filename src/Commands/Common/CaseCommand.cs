using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class CaseCommand
    {
        [Command("case")]
        public static async Task ExecuteAsync(CommandContext context, CaseType caseType, string content)
        {
            List<string> output = [];
            foreach (string line in content.Split('\n'))
            {
                output.Add(caseType switch
                {
                    CaseType.Upper => line.ToUpper(CultureInfo.InvariantCulture),
                    CaseType.Lower => line.ToLower(CultureInfo.InvariantCulture),
                    CaseType.Title => line.Titleize(),
                    CaseType.Snake => line.Underscore(),
                    CaseType.Pascal => line.Pascalize(),
                    CaseType.Camel => line.Camelize(),
                    _ => throw new ArgumentOutOfRangeException(nameof(caseType), caseType, null)
                });
            }

            await context.RespondAsync(Formatter.BlockCode(string.Join('\n', output)));
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
