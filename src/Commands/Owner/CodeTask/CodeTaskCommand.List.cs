using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

namespace OoLunar.Tomoe.Commands.Owner
{
    public sealed partial class CodeTaskCommand
    {
        /// <summary>
        /// Lists all code tasks in the guild.
        /// </summary>
        /// <param name="guildId">The guild to list code tasks from.</param>
        [Command("list"), RequireGuild]
        public static async ValueTask ListAsync(CommandContext context, ulong? guildId = null)
        {
            guildId ??= context.Guild!.Id;

            // Get all code tasks.
            List<Page> pages = [];
            await foreach (CodeTaskModel codeTaskModel in CodeTaskModel.GetAllGuildAsync(guildId ?? context.Guild!.Id))
            {
                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.AddField("Name", codeTaskModel.Name, true);
                embedBuilder.AddField("Created At", Formatter.Timestamp(codeTaskModel.Id.Time, TimestampFormat.LongDateTime), true);
                embedBuilder.AddField("Id", $"`{codeTaskModel.Id}`", false);

                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.AddEmbed(embedBuilder);
                if (codeTaskModel.Code.Length < 2000 - 12)
                {
                    messageBuilder.Content = $"```cs\n{codeTaskModel.Code}\n```";
                }
                else
                {
                    MemoryStream memoryStream = new();
                    await memoryStream.WriteAsync(Encoding.UTF8.GetBytes(codeTaskModel.Code));
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    messageBuilder.AddFile($"{codeTaskModel.Name}.cs", memoryStream, AddFileOptions.ResetStream);
                }

                pages.Add(new Page(messageBuilder, codeTaskModel.Name));
            }

            // If there are no code tasks, respond with a message.
            if (pages.Count == 0)
            {
                await context.RespondAsync("No code tasks found.");
                return;
            }

            // Send the pages.
            await context.PaginateAsync(pages);
        }
    }
}
