using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Utils.Converters
{
    public class ReminderConverter : IArgumentConverter<Reminder>
    {
        public async Task<Optional<Reminder>> ConvertAsync(string value, CommandContext context)
        {
            using IServiceScope scope = context.Services.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            if (value[0] == '#')
            {
                value = value[1..];
            }

            bool convertedSuccessfully = int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out int reminderId);
            if (!convertedSuccessfully)
            {
                await Program.SendMessage(context, $"{Formatter.InlineCode(value)} is not a valid reminder id!");
                return Optional.FromNoValue<Reminder>();
            }
            Reminder reminder = await database.Reminders.AsNoTracking().FirstOrDefaultAsync(reminder => reminder.LogId == reminderId && reminder.GuildId == context.Guild.Id && reminder.UserId == context.User.Id);
            database.Dispose();
            if (reminder == null)
            {
                await Program.SendMessage(context, $"Reminder #{reminderId} not found!");
                return Optional.FromNoValue<Reminder>();
            }
            else
            {
                return Optional.FromValue(reminder);
            }
        }
    }
}