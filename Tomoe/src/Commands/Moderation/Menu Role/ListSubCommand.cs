using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands.Moderation
{
    public sealed partial class MenuRoleCommand : ApplicationCommandModule
    {
        [SlashCommand("list", "Shows all autoreactions on a channel.")]
        public Task ListAsync(InteractionContext context, [Option("channel", "Which channel to view the autoreactions on.")] DiscordChannel channel = null) => throw new NotImplementedException();
    }
}
