namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Tomoe.Db;

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