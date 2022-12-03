using System.Data;
using System.Threading.Tasks;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class CalculateCommand : BaseCommand
    {
        private static readonly DataTable _dataTable = new();

        [Command("calculate", "calc")]
        public static Task ExecuteAsync(CommandContext context, params string[] expression)
        {
            object? value = _dataTable.Compute(string.Join(" ", expression), null);
            return value is not decimal decimalValue
                ? context.ReplyAsync($"Result: {value:N0}")
                : context.ReplyAsync($"Result: {decimalValue:N}");
        }
    }
}
