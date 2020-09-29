using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Tomoe.Utils.Dialog;

namespace Tomoe {

    public enum Event {
        MessageUpdated,
        MessageDeleted,
        UserJoin,
        UserLeave,
        Antiraid
    }

    public static class Listeners {
        private static Dictionary<ulong, System.Timers.Timer> timers = new Dictionary<ulong, System.Timers.Timer>();
        private static Dictionary<ulong, int> joinRate = new Dictionary<ulong, int>();

        public static async Task MessageUpdate(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) {
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            ulong? loggingChannelID = Tomoe.Utils.Cache.Guild.GetLoggingChannel(guild.Id, Event.MessageUpdated);
            Utils.Dialog.Context dialogContext = new Utils.Dialog.Context();
            dialogContext.Guild = guild;
            dialogContext.Issuer = after.Author;
            dialogContext.NewMessage = after;
            dialogContext.Channel = after.Channel;
            dialogContext.Error = Program.Dialogs.Message.Events.MessageUpdated;

            if (before.HasValue) dialogContext.OldMessage = before.Value;

            if (after.EditedTimestamp.HasValue) dialogContext.Timestamp = after.EditedTimestamp.Value;
            else dialogContext.Timestamp = after.Timestamp;

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

        public static async Task AntiRaid(SocketGuildUser newUser) {
            if (joinRate.TryGetValue(newUser.Guild.Id, out _) == false) joinRate[newUser.Guild.Id] = 1;
            if (Utils.Cache.Antiraid.IsActivated(newUser.Guild.Id).Value && !newUser.IsBot) {
                Context dialogContext = new Context();
                dialogContext.Guild = newUser.Guild;
                // TODO: Set logging channel.
                //dialogContext.Channel
                dialogContext.Issuer = Program.Client.CurrentUser;
                dialogContext.Victim = newUser;
                dialogContext.UserAction = Context.Action.AntiraidBan;
                dialogContext.Reason = Program.Dialogs.Message.Events.AntiraidBan;
                await dialogContext.SendDM();
                //await dialogContext.SendChannel();
                newUser.KickAsync(Program.Dialogs.Message.Events.AntiraidBan);
            } else {
                // TODO: Finish logging channels and log the join.
                if (timers.TryGetValue(newUser.Guild.Id, out _) == false && ++joinRate[newUser.Guild.Id] > 5) {
                    timers[newUser.Guild.Id] = new System.Timers.Timer();
                    // Stores the interval in seconds. Convert to milliseconds as Timer requests.
                    timers[newUser.Guild.Id].Interval = (Utils.Cache.Antiraid.GetInterval(newUser.Guild.Id) ?? 300) * 1000;
                    timers[newUser.Guild.Id].Elapsed += async delegate(object sender, System.Timers.ElapsedEventArgs e) {
                        Tomoe.Utils.Cache.Antiraid.SetActivated(newUser.Guild.Id, false);
                        timers[newUser.Guild.Id].Dispose();
                        joinRate[newUser.Guild.Id] = 0;
                        timers.Remove(newUser.Guild.Id);
                        System.Console.WriteLine($"{System.DateTime.Now} Raid ended.");
                    };
                    Tomoe.Utils.Cache.Antiraid.SetActivated(newUser.Guild.Id, true);
                    timers[newUser.Guild.Id].Start();
                    System.Console.WriteLine($"{System.DateTime.Now} Raid started.");
                } else if (timers.TryGetValue(newUser.Guild.Id, out _) == true) {
                    timers[newUser.Guild.Id].Stop();
                    timers[newUser.Guild.Id].Start();
                    System.Console.WriteLine("Raid continuing");
                }
            }
        }
    }
}