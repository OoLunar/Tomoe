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
    }
}