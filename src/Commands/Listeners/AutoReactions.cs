namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class AutoReactions
    {
        public static async Task Handler(DiscordClient discordClient, MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs.Guild == null)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            foreach (AutoReaction autoReaction in database.AutoReactions.Where(autoReaction => autoReaction.GuildId == messageCreateEventArgs.Guild.Id && autoReaction.ChannelId == messageCreateEventArgs.Channel.Id))
            {
                await messageCreateEventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(discordClient, autoReaction.EmojiName, true));
            }
        }
    }
}
