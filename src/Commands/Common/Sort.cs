using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class Sort : BaseCommandModule
    {
        [Command("sort")]
        public Task SortAsync(CommandContext context, bool trimLines = false, [Description("A line seperated list."), RemainingText] string? list = null)
        {
            if (list == null)
            {
                return Task.CompletedTask;
            }

            List<string> lines = new(list.Split('\n'));
            if (trimLines)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i] = lines[i].Trim();
                }
            }
            lines.Sort();

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.WithFile("Sorted List.txt", new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", lines))));
            return context.RespondAsync(messageBuilder);
        }
    }
}
