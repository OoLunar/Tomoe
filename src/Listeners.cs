using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Tomoe.Utils;

namespace Tomoe {

    public enum Event {
        MessageUpdated,
        MessageDeleted,
    }

    public static class Listeners {
        public static async Task MessageUpdate(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) {
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            ulong? loggingChannelID = Tomoe.Utils.Cache.Guild.GetLoggingChannel(guild.Id, Event.MessageUpdated);

            Utils.DialogContext dialogContext = new Utils.DialogContext();
            dialogContext.Guild = guild;
            dialogContext.Issuer = after.Author;
            if (before.HasValue) dialogContext.OldMessage = before.Value;
            dialogContext.NewMessage = after;
            if (after.EditedTimestamp.HasValue)
                dialogContext.Timestamp = after.EditedTimestamp.Value;
            else
                dialogContext.Timestamp = after.Timestamp;
            dialogContext.Error = Program.Dialogs.Message.Events.MessageUpdated;

            if (loggingChannelID.HasValue) {
                SocketTextChannel loggingChannel = guild.GetTextChannel(loggingChannelID.Value);
                dialogContext.Channel = loggingChannel;
                try {
                    await dialogContext.SendChannel();
                } catch (System.Exception error) { System.Console.WriteLine(error); }
            }
        }

        public static async Task MessageDelete(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel) {
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            ulong? loggingChannelID = Tomoe.Utils.Cache.Guild.GetLoggingChannel(guild.Id, Event.MessageDeleted);
            if (loggingChannelID.HasValue) {
                SocketTextChannel loggingChannel = guild.GetTextChannel(loggingChannelID.Value);
                try {
                    await loggingChannel.SendMessageAsync(Program.Dialogs.Message.Events.MessageDeleted);
                } catch (System.Exception error) { System.Console.WriteLine(error); }
            }
        }
    }
}