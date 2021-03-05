using System.Linq;
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
	public class TagConverter : IArgumentConverter<Tag>
	{
		public async Task<Optional<Tag>> ConvertAsync(string value, CommandContext context)
		{
			value = value.Trim().ToLowerInvariant();
			using IServiceScope scope = context.Services.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold("[Error: Failed to get tag, guild is not in the database!]"));
				return Optional.FromNoValue<Tag>();
			}

			Tag tag = guild.Tags.FirstOrDefault(tag => tag.Name == value || tag.AliasTo == value);
			if (tag == null)
			{
				_ = await Program.SendMessage(context, $"Tag {Formatter.InlineCode(value)} not found!");
				return Optional.FromNoValue<Tag>();
			}

			return Optional.FromValue(tag);
		}
	}
}
