using System.Data;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class CalculateCommand
    {
        private static readonly DataTable _dataTable = new();

        [Command("calculate"), TextAlias("calc")]
        public static async Task ExecuteAsync(CommandContext context, params string[] expression)
        {
            object? value = _dataTable.Compute(string.Join(" ", expression), null);
            await context.RespondAsync(value is not decimal decimalValue
                ? $"Result: {value:N0}"
                : $"Result: {decimalValue:N}");
        }
    }
}
