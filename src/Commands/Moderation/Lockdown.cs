using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    [Group("lockdown"), Description("Locks a channel, role or the entire server."), Aliases("lock_down", "lock"), RequireGuild, RequireUserPermissions(Permissions.ManageRoles | Permissions.ManageChannels), RequireBotPermissions(Permissions.ManageRoles | Permissions.ManageChannels)]
    public class Lockdown : BaseCommandModule
    {
        [Command("channel")]
        public async Task Channel(CommandContext context, DiscordChannel channel, [RemainingText] string lockReason = Constants.MissingReason)
        {
            await Api.Moderation.Lockdown.Channel(context.Guild, true, context.User.Id, null, new() { channel }, null, lockReason);
            await Program.SendMessage(context, $"Channel {channel.Mention} successfully locked. All roles below me cannot send messages or react. To undo this, run `>>unlock channel`");
        }

        [Command("channel")]
        public async Task Channel(CommandContext context, [RemainingText] string lockReason = Constants.MissingReason) => await Channel(context, context.Channel, lockReason);

        [Command("server")]
        public async Task Server(CommandContext context, [RemainingText] string lockReason = Constants.MissingReason)
        {
            await Api.Moderation.Lockdown.Server(context.Guild, context.User.Id, lockReason);
            await Program.SendMessage(context, $"Server successfully locked. All roles below me cannot send messages or react. To undo this, run `>>unlock server`");
        }

        [Command("role")]
        public async Task Role(CommandContext context, DiscordRole role, DiscordChannel channel = null, [RemainingText] string lockReason = Constants.MissingReason)
        {
            if (channel == null)
            {
                await Api.Moderation.Lockdown.Role(context.Guild, role, context.User.Id, lockReason);
            }
            else
            {
                await Api.Moderation.Lockdown.Channel(context.Guild, true, context.User.Id, null, new() { channel }, new() { role }, lockReason);
            }
        }

        [Command("bots")]
        public async Task Bots(CommandContext context, DiscordChannel channel = null, [RemainingText] string lockReason = Constants.MissingReason)
        {
            if (channel == null)
            {
                await Api.Moderation.Lockdown.Bots(context.Guild, context.User.Id, lockReason);
                await Program.SendMessage(context, $"All bots are locked across the server. Reason: {lockReason}");
            }
            else
            {
                await Api.Moderation.Lockdown.Bots(context.Guild, context.User.Id, lockReason, channel);
                await Program.SendMessage(context, $"All bots are locked in channel {channel.Mention}. Reason: ${lockReason}");
            }
        }
    }
}