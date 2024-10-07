using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class UserSettingsCache
    {
        private readonly Dictionary<ulong, UserSettingsModel> _cache = [];

        public async ValueTask<UserSettingsModel> GetAsync(ulong userId)
        {
            if (_cache.TryGetValue(userId, out UserSettingsModel? settings))
            {
                return settings;
            }

            UserSettingsModel model = await UserSettingsModel.GetUserSettingsAsync(userId) ?? new UserSettingsModel()
            {
                UserId = userId,
                Culture = CultureInfo.InvariantCulture,
                Timezone = TimeZoneInfo.Utc
            };

            _cache.Add(userId, model);
            return model;
        }
    }
}
