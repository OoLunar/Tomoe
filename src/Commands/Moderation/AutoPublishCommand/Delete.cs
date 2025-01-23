using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed partial class AutoPublishCommand
    {
        /// <summary>
        /// Deletes the auto publish settings for a channel.
        /// </summary>
        /// <param name="channel">The channel to stop listening for messages in.</param>
        [Command("delete")]
        public static async ValueTask DeleteAsync(CommandContext context, DiscordChannel channel)
        {
            if (!await AutoPublishModel.ExistsAsync(context.Guild!.Id, channel.Id))
            {
                await context.RespondAsync($"Auto publish is not enabled in <@{channel.Id}>.");
                return;
            }

            await AutoPublishModel.DeleteAsync(context.Guild!.Id, channel.Id);
            await context.RespondAsync($"Auto publish has been disabled for <@{channel.Id}>.");
        }
    }
}
