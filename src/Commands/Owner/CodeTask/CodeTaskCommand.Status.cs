using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using Humanizer;
using OoLunar.Tomoe.CodeTasks;

namespace OoLunar.Tomoe.Commands.Owner
{
    public sealed partial class CodeTaskCommand
    {
        /// <summary>
        /// Gets the status of a code task.
        /// </summary>
        /// <param name="id">The id of the code task to get the status of.</param>
        [Command("status")]
        public static async ValueTask StatusAsync(CommandContext context, Ulid id)
        {
            TaskStatus? status = CodeTaskRunner.GetStatus(id);
            await context.RespondAsync(status is null
                ? $"Code task `{id}` not found."
                : $"Code task `{id}` status: {status.Value.Humanize()}"
            );
        }
    }
}
