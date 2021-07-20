namespace Tomoe.Commands
{
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Threading.Tasks;

    public partial class Moderation : SlashCommandModule
    {
        public partial class MenuRoles : SlashCommandModule
        {
            [SlashCommand("list", "Shows all autoreactions on a channel.")]
            public async Task List(InteractionContext context, [Option("channel", "Which channel to view the autoreactions on.")] DiscordChannel channel = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}