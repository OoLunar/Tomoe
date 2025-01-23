using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed partial class AutoPublishCommand
    {
        /// <summary>
        /// Lists all channels that are subscribed to auto publish.
        /// </summary>
        [Command("list")]
        public static async ValueTask ListAsync(CommandContext context)
        {
            List<string> channels = [];
            await foreach (AutoPublishModel channel in AutoPublishModel.GetAllGuildAsync(context.Guild!.Id))
            {
                channels.Add($"- <@{channel.ChannelId}>");
            }

            if (channels.Count == 0)
            {
                await context.RespondAsync("No channels are subscribed to auto publish.");
                return;
            }

            await context.RespondAsync($"Channels subscribed to auto publish:\n{string.Join('\n', channels)}");
        }
    }
}
