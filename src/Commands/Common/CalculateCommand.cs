using System.Data;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// 1 + 1 = 2, quick maths.
    /// </summary>
    public sealed class CalculateCommand
    {
        private static readonly DataTable _dataTable = new();

        /// <summary>
        /// Preforms basic arithmetic calculations.
        /// </summary>
        [Command("calculate"), TextAlias("calc")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string expression)
        {
            object? value = _dataTable.Compute(expression, null);
            return context.RespondAsync(value is decimal decimalValue
                ? $"{decimalValue:N2}"
                : $"{value:N0}"
            );
        }
    }
}
