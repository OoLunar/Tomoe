namespace Tomoe.Commands
{
    using System;
    using System.Threading.Tasks;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;

    public partial class Moderation : ApplicationCommandModule
    {
        public partial class MenuRoles : ApplicationCommandModule
        {
            [SlashCommand("list", "Shows all autoreactions on a channel.")]
            public Task List(InteractionContext context, [Option("channel", "Which channel to view the autoreactions on.")] DiscordChannel channel = null) => throw new NotImplementedException();
        }
    }
}
