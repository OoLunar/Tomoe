using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class CaseCommand : BaseCommand
    {
        [Command("case")]
        public static Task ExecuteAsync(CommandContext context, CaseType caseType, string content)
        {
            List<string> output = new();
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

            return context.ReplyAsync(Formatter.BlockCode(string.Join('\n', output)));
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
