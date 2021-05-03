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
	public class TagConverter : IArgumentConverter<Tag>
	{
		public async Task<Optional<Tag>> ConvertAsync(string value, CommandContext context)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Tag tag = await database.Tags.AsNoTracking().FirstOrDefaultAsync(tag => tag.Name == value.ToLowerInvariant().Trim() && tag.GuildId == context.Guild.Id);

			if (tag == null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: Tag `{value.ToLowerInvariant()}` not found."));
				return Optional.FromNoValue<Tag>();
			}

			return Optional.FromValue(tag);
		}
	}
}
