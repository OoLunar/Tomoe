using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe
{
    public static partial class ExtensionMethods
    {
        public static async ValueTask<CultureInfo> GetCultureAsync(this AbstractContext context, DiscordUser? user = null)
        {
            user ??= context.User;
            UserSettingsCache cache = context.ServiceProvider.GetRequiredService<UserSettingsCache>();
            return (await cache.GetAsync(user.Id)).Culture;
        }

        public static async ValueTask<TimeZoneInfo> GetTimeZoneAsync(this AbstractContext context, DiscordUser? user = null)
        {
            user ??= context.User;
            UserSettingsCache cache = context.ServiceProvider.GetRequiredService<UserSettingsCache>();
            return (await cache.GetAsync(user.Id)).Timezone;
        }

        public static string GetDisplayName(this DiscordUser user)
        {
            if (user is DiscordMember member)
            {
                return member.DisplayName;
            }
            else if (!string.IsNullOrEmpty(user.GlobalName))
            {
                return user.GlobalName;
            }
            else if (user.Discriminator == "0")
            {
                return user.Username;
            }

            return $"{user.Username}#{user.Discriminator}";
        }

        public static string PluralizeCorrectly(this string str) => str.Length == 0 ? str : str[^1] switch
        {
            // Ensure it doesn't already end with `'s`
            's' when str.Length > 1 && str[^2] == '\'' => str,
            's' => str + '\'',
            '\'' => str + 's',
            _ => str + "'s"
        };
    }
}
