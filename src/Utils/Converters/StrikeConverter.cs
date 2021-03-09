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
	public class StrikeConverter : IArgumentConverter<Strike>
	{
		public async Task<Optional<Strike>> ConvertAsync(string value, CommandContext context)
		{
			using IServiceScope scope = context.Services.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			if (value[0] == '#') value = value[1..];
			bool convertedSuccessfully = int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out int strikeId);
			if (!convertedSuccessfully)
			{
				_ = await Program.SendMessage(context, $"{Formatter.InlineCode(value)} is not a valid strike id!");
				return Optional.FromNoValue<Strike>();
			}
			Strike strike = await database.Strikes.AsNoTracking().FirstOrDefaultAsync(strike => strike.Id == strikeId && strike.GuildId == context.Guild.Id);
			database.Dispose();
			if (strike == null)
			{
				_ = await Program.SendMessage(context, $"Strike #{strikeId} not found!");
				return Optional.FromNoValue<Strike>();
			}
			else return Optional.FromValue(strike);
		}
	}
}
