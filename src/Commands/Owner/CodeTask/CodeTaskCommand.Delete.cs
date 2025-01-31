using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using OoLunar.Tomoe.CodeTasks;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Owner
{
    public sealed partial class CodeTaskCommand
    {
        /// <summary>
        /// Deletes a code task.
        /// </summary>
        /// <param name="id">The id of the code task to delete.</param>
        [Command("delete")]
        public static async ValueTask DeleteAsync(CommandContext context, Ulid id)
        {
            if (CodeTaskRunner.GetStatus(id) is null)
            {
                await context.RespondAsync($"Code task `{id}` not found.");
                return;
            }

            bool found = await CodeTaskModel.DeleteAsync(id);
            if (!found)
            {
                await context.RespondAsync($"Code task `{id}` not found.");
                return;
            }

            try
            {
                await CodeTaskRunner.StopAsync(id);
            }
            catch (Exception error)
            {
                await context.RespondAsync($"Code task `{id}` was removed from the database but could not be stopped: {error.Message}");
                return;
            }

            await context.RespondAsync($"Code task `{id}` deleted.");
        }
    }
}
