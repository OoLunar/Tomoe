using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Tomoe.Utils {
    public static class ExtensionMethods {

        private enum TimeSpanElement {
            Millisecond,
            Second,
            Minute,
            Hour,
            Day
        }

        public static string ToFriendlyDisplay(this TimeSpan timeSpan, int maxNrOfElements) {
            maxNrOfElements = Math.Max(Math.Min(maxNrOfElements, 5), 1);
            var parts = new [] {
                    Tuple.Create(TimeSpanElement.Day, timeSpan.Days),
                        Tuple.Create(TimeSpanElement.Hour, timeSpan.Hours),
                        Tuple.Create(TimeSpanElement.Minute, timeSpan.Minutes),
                        Tuple.Create(TimeSpanElement.Second, timeSpan.Seconds),
                        Tuple.Create(TimeSpanElement.Millisecond, timeSpan.Milliseconds)
                }
                .SkipWhile(i => i.Item2 <= 0)
                .Take(maxNrOfElements);

            return string.Join(", ", parts.Select(p => string.Format("{0} {1}{2}", p.Item2, p.Item1, p.Item2 != 1 ? "s" : string.Empty)));
        }

        public static string ToPastString(this Tomoe.Utils.DialogContext.Action action) {
            switch (action) {
                case Tomoe.Utils.DialogContext.Action.Ban:
                    return "banned";
                case Tomoe.Utils.DialogContext.Action.Kick:
                    return "kicked";
                case Tomoe.Utils.DialogContext.Action.Mute:
                    return "muted";
                case Tomoe.Utils.DialogContext.Action.NoMeme:
                    return "no memed";
                case Tomoe.Utils.DialogContext.Action.Strike:
                    return "striken";
                case Tomoe.Utils.DialogContext.Action.TempBan:
                    return "temporarily banned";
                case Tomoe.Utils.DialogContext.Action.TempMute:
                    return "temporarily muted";
                case Tomoe.Utils.DialogContext.Action.TempNoMeme:
                    return "temporarily no memed";
                case Tomoe.Utils.DialogContext.Action.UnBan:
                    return "unbanned";
                case Tomoe.Utils.DialogContext.Action.UnMute:
                    return "unmuted";
                case Tomoe.Utils.DialogContext.Action.UnNoMeme:
                    return "no memed no longer";
                default:
                    return action.ToString();
            }
        }

        public static System.Collections.Generic.IReadOnlyCollection<SocketRole> ExceptEveryoneRole(this System.Collections.Generic.IReadOnlyCollection<SocketRole> roles) {
            List<SocketRole> newListRoles = new List<SocketRole> { };
            foreach (SocketRole role in roles)
                if (!role.IsEveryone) newListRoles.Add(role);
            return newListRoles.ToImmutableArray();
        }

        public static string GetCommonName(this IGuildUser guildMember) {
            if (guildMember != null) {
                if (guildMember.Nickname != null) return guildMember.Nickname;
                else return guildMember.Username;
            } else return null;
        }

        public static string GetCommonName(this IUser user) => user.Username;

        public static Embed DialogSetParams(dynamic modifyEmbed, DialogContext context) {
            // Replace any empty, missing or $null by removing the field.
            EmbedBuilder embed = new EmbedBuilder();
            if (modifyEmbed.Author != null && modifyEmbed.Author != "$null") switch (modifyEmbed.Author) {
                case "$issuer":
                    embed.Author = (EmbedAuthorBuilder) context.Issuer;
                    break;
                case "$victim":
                    embed.Author = (EmbedAuthorBuilder) context.Victim;
                    break;
                case "$self":
                    embed.Author = (EmbedAuthorBuilder) ((IUser) Program.Client.CurrentUser);
                    break;
            }
            if (modifyEmbed.Color != null && modifyEmbed.Color != "$null") embed.Color = modifyEmbed.Color;
            if (modifyEmbed.Description != null && modifyEmbed.Description != "$null") embed.Description = DialogSetParams(modifyEmbed.Description, context);
            if (modifyEmbed.Footer != null && modifyEmbed.Footer != "$null") embed.Footer = modifyEmbed.Footer;
            if (modifyEmbed.ThumbnailUrl != null && modifyEmbed.ThumbnailUrl != "$null") embed.ThumbnailUrl = modifyEmbed.ThumbnailUrl;
            if (modifyEmbed.Title != null && modifyEmbed.Title != "$null") embed.Title = modifyEmbed.Title;
            return embed.Build();
        }
    }
}

public static class AsyncHelpers {
    /// <summary>
    /// Execute's an async Task<T> method which has a void return value synchronously
    /// </summary>
    /// <param name="task">Task<T> method to execute</param>
    public static void RunSync(Func<Task> task) {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        synch.Post(async _ => {
            try {
                await task();
            } catch (Exception e) {
                synch.InnerException = e;
                throw;
            } finally {
                synch.EndMessageLoop();
            }
        }, null);
        synch.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    /// <summary>
    /// Execute's an async Task<T> method which has a T return type synchronously
    /// </summary>
    /// <typeparam name="T">Return Type</typeparam>
    /// <param name="task">Task<T> method to execute</param>
    /// <returns></returns>
    public static T RunSync<T>(Func<Task<T>> task) {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        T ret = default(T);
        synch.Post(async _ => {
            try {
                ret = await task();
            } catch (Exception e) {
                synch.InnerException = e;
                throw;
            } finally {
                synch.EndMessageLoop();
            }
        }, null);
        synch.BeginMessageLoop();
        SynchronizationContext.SetSynchronizationContext(oldContext);
        return ret;
    }

    private class ExclusiveSynchronizationContext : SynchronizationContext {
        private bool done;
        public Exception InnerException { get; set; }
        readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
        readonly Queue<Tuple<SendOrPostCallback, object>> items = new Queue<Tuple<SendOrPostCallback, object>>();

        public override void Send(SendOrPostCallback d, object state) =>
            throw new NotSupportedException("We cannot send to our same thread");

        public override void Post(SendOrPostCallback d, object state) {
            lock(items) {
                items.Enqueue(Tuple.Create(d, state));
            }
            workItemsWaiting.Set();
        }

        public void EndMessageLoop() {
            Post(_ => done = true, null);
        }

        public void BeginMessageLoop() {
            while (!done) {
                Tuple<SendOrPostCallback, object> task = null;
                lock(items) {
                    if (items.Count > 0) {
                        task = items.Dequeue();
                    }
                }
                if (task != null) {
                    task.Item1(task.Item2);
                    // the method threw an exeption
                    if (InnerException != null) {
                        throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                    }
                } else {
                    workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy() => this;
    }
}