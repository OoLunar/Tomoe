namespace Tomoe.Commands
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using Tomoe.Db;

    public partial class Listeners
    {
        public static async Task AutoReactions(DiscordClient discordClient, MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs.Guild == null)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            foreach (AutoReaction autoReaction in database.AutoReactions.Where(autoReaction => autoReaction.GuildId == messageCreateEventArgs.Guild.Id && autoReaction.ChannelId == messageCreateEventArgs.Channel.Id))
            {
                DiscordEmoji discordEmoji = autoReaction.EmojiName switch
                {
                    _ when DiscordEmoji.TryFromName(discordClient, autoReaction.EmojiName, out DiscordEmoji emoji) => emoji,
                    _ when DiscordEmoji.TryFromUnicode(autoReaction.EmojiName, out DiscordEmoji emoji) => emoji,
                    _ when DiscordEmoji.TryFromGuildEmote(discordClient, ulong.Parse(autoReaction.EmojiName, CultureInfo.InvariantCulture), out DiscordEmoji emoji) => emoji,
                    _ => throw new ArgumentException("Not an emoji")
                };
                await messageCreateEventArgs.Message.CreateReactionAsync(discordEmoji);
            }
        }
    }
}
