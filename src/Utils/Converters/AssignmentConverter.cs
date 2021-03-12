using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Public;
using Tomoe.Db;

namespace Tomoe.Utils.Converters
{
	public class AssignmentConverter : IArgumentConverter<Assignment>
	{
		public async Task<Optional<Assignment>> ConvertAsync(string value, CommandContext context)
		{
			if (value[0] == '#') value = value[1..];
			bool convertedSuccessfully = int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out int assignmentId);
			if (!convertedSuccessfully)
			{
				_ = await Program.SendMessage(context, $"{Formatter.InlineCode(value)} is not a valid strike id!");
				return Optional.FromNoValue<Assignment>();
			}

			// Search local reminders first
			Assignment assignment = Assignments.AssignmentsList.FirstOrDefault(assignment => assignment.Id == assignmentId && assignment.UserId == context.User.Id);
			// If local reminders doesn't have it, search the database
			if (assignment == null)
			{
				using IServiceScope scope = context.Services.CreateScope();
				Database database = scope.ServiceProvider.GetService<Database>();
				assignment = await database.Assignments.AsNoTracking().FirstOrDefaultAsync(assignment => assignment.Id == assignmentId && assignment.UserId == context.User.Id);
				database.Dispose();
			}

			// Inform the user that the assignment cannot be found locally or on the database
			if (assignment == null)
			{
				_ = await Program.SendMessage(context, $"Assignment #{assignmentId} not found!");
				return Optional.FromNoValue<Assignment>();
			}
			else return Optional.FromValue(assignment);
		}
	}
}
