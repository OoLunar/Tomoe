using System.Data;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Preforms basic arithmetic calculations.
    /// </summary>
    public sealed class CalculateCommand
    {
        private static readonly DataTable _dataTable = new();

        /// <inheritdoc cref="CalculateCommand"/>
        [Command("calculate"), TextAlias("calc")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string expression) => context.RespondAsync($"{_dataTable.Compute(expression, null):N0}");
    }
}
