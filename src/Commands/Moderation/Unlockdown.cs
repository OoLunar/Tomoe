using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    [Group("unlock"), Description("Unlocks a channel, role or the entire server."), Aliases("unlock_down", "unlockdown"), RequireGuild, RequireUserPermissions(Permissions.ManageRoles | Permissions.ManageChannels), RequireBotPermissions(Permissions.ManageRoles | Permissions.ManageChannels)]
    public class Unlockdown : BaseCommandModule
    {
        public Database Database { private get; set; }

        [Command("channel")]
        public async Task Channel(CommandContext context, DiscordChannel channel, [RemainingText] string lockReason = Constants.MissingReason)
        {
            await Api.Moderation.Unlockdown.Channel(context.Guild, true, context.User.Id, null, new() { channel }, null, lockReason);
            await Program.SendMessage(context, $"Channel {channel.Mention} successfully unlocked. Permissions were restored to what they were before.");
        }

        [Command("channel")]
        public async Task Channel(CommandContext context, [RemainingText] string lockReason = Constants.MissingReason) => await Channel(context, context.Channel, lockReason);

        [Command("server")]
        public async Task Server(CommandContext context, [RemainingText] string lockReason = Constants.MissingReason)
        {
            await Api.Moderation.Unlockdown.Server(context.Guild, context.User.Id, lockReason);
            await Program.SendMessage(context, $"Server successfully locked. Permissions were restored to what they were before.");
        }

        [Command("role")]
        public async Task Role(CommandContext context, DiscordRole role, DiscordChannel channel = null, [RemainingText] string lockReason = Constants.MissingReason)
        {
            if (channel == null)
            {
                await Api.Moderation.Unlockdown.Role(context.Guild, role, context.User.Id, lockReason);
            }
            else
            {
                await Api.Moderation.Unlockdown.Channel(context.Guild, true, context.User.Id, null, new() { channel }, new() { role }, lockReason);
            }
        }

        [Command("bots")]
        public async Task Bots(CommandContext context, DiscordChannel channel = null, [RemainingText] string lockReason = Constants.MissingReason)
        {
            if (channel == null)
            {
                await Api.Moderation.Unlockdown.Bots(context.Guild, context.User.Id, lockReason);
                await Program.SendMessage(context, $"All bots are locked across the server. Reason: {lockReason}");
            }
            else
            {
                await Api.Moderation.Unlockdown.Bots(context.Guild, context.User.Id, lockReason, channel);
                await Program.SendMessage(context, $"All bots are locked in channel {channel.Mention}. Reason: ${lockReason}");
            }
        }
    }
}