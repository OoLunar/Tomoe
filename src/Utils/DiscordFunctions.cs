using System.Collections.Generic;
using System.Collections.Immutable;
using Discord;
using Discord.WebSocket;
using Tomoe.Utils.Cache;

namespace Tomoe.Utils {
    public class DiscordFunctions {
        public static string GetCommonName(IGuildUser guildMember) {
            string nickname;
            if (guildMember.Nickname == null) nickname = guildMember.Username;
            else nickname = guildMember.Nickname;
            return nickname;
        }
    }

    public static class ExtensionMethods {
        public static System.Collections.Generic.IReadOnlyCollection<SocketRole> ExceptEveryoneRole(this System.Collections.Generic.IReadOnlyCollection<SocketRole> roles) {
            List<SocketRole> newListRoles = new List<SocketRole> { };
            foreach (SocketRole role in roles)
                if (!role.IsEveryone) newListRoles.Add(role);
            return newListRoles.ToImmutableArray();
        }

        public static string GetRandomValue(this Dictionary<string, string[]> dict, string key) {
            System.Random randNum = new System.Random();
            if (dict[key] == null) System.Console.WriteLine($"Unknown Key: {key}");
            return dict[key][randNum.Next(0, dict[key].Length)];
        }

        public static string DialogSetParams(this string modifyString, IGuildUser issuer, IGuildUser victim, string reason) {
            return modifyString
                .Replace("$victim_id", victim.Id.ToString()).Replace("$victim", victim.Mention).Replace("$victim_nick", DiscordFunctions.GetCommonName(victim))
                .Replace("$issuer", issuer.Mention).Replace("$issuer_id", issuer.Id.ToString()).Replace("$issuer_nick", DiscordFunctions.GetCommonName(issuer))
                .Replace("$reason", reason).Replace("\\n", "\n");
        }
    }
}