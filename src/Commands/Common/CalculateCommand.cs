using System.Data;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class CalculateCommand
    {
        private static readonly DataTable _dataTable = new();

        [Command("calculate"), TextAlias("calc")]
        public static ValueTask ExecuteAsync(CommandContext context, params string[] expression)
        {
            object? value = _dataTable.Compute(string.Join(" ", expression), null);
            return context.RespondAsync(value is decimal decimalValue
                ? $"Result: {decimalValue:N}"
                : $"Result: {value:N0}");
        }
    }
}
