using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed partial class AutoPublishCommand
    {
        /// <summary>
        /// Subscribes a channel to automatically publish all posted messages in.
        /// </summary>
        /// <param name="channel">The channel to listen for messages in.</param>
        [Command("create"), DefaultGroupCommand]
        public static async ValueTask CreateAsync(CommandContext context, DiscordChannel channel)
        {
            if (await AutoPublishModel.ExistsAsync(context.Guild!.Id, channel.Id))
            {
                await context.RespondAsync($"Auto publish is already enabled in <@{channel.Id}>.");
                return;
            }

            await AutoPublishModel.CreateAsync(context.Guild!.Id, channel.Id);
            await context.RespondAsync($"Auto publish has been enabled for <@{channel.Id}>.");
        }
    }
}
