using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Utils.Converters
{
	public class ModLogConverter : IArgumentConverter<ModLog>
	{
		public async Task<Optional<ModLog>> ConvertAsync(string value, CommandContext context)
		{
			using IServiceScope scope = context.Services.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			if (value[0] == '#') value = value[1..];
			bool convertedSuccessfully = int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out int modLogId);
			if (!convertedSuccessfully)
			{
				_ = await Program.SendMessage(context, $"{Formatter.InlineCode(value)} is not a valid modlog id!");
				return Optional.FromNoValue<ModLog>();
			}
			ModLog modLog = await database.ModLogs.AsNoTracking().FirstOrDefaultAsync(modLog => modLog.Id == modLogId && modLog.GuildId == context.Guild.Id);
			database.Dispose();
			if (modLog == null)
			{
				_ = await Program.SendMessage(context, $"ModLog #{modLogId} not found!");
				return Optional.FromNoValue<ModLog>();
			}
			else return Optional.FromValue(modLog);
		}
	}
}
