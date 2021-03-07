using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe
{
	public class ReminderConverter : IArgumentConverter<Assignment>
	{
		public async Task<Optional<Assignment>> ConvertAsync(string value, CommandContext context)
		{
			using IServiceScope scope = context.Services.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			if (value[0] == '#') value = value[1..];
			bool convertedSuccessfully = int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out int assignmentId);
			if (!convertedSuccessfully)
			{
				_ = await Program.SendMessage(context, $"{Formatter.InlineCode(value)} is not a valid strike id!");
				return Optional.FromNoValue<Assignment>();
			}
			Assignment assignment = await database.Assignments.FirstOrDefaultAsync(assignment => assignment.Id == assignmentId);
			if (assignment == null)
			{
				_ = await Program.SendMessage(context, $"Assignment #{assignmentId} not found!");
				return Optional.FromNoValue<Assignment>();
			}
			else return Optional.FromValue(assignment);
		}
	}
}
