using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Tomoe.Models;

namespace Tomoe.Converters
{
    public class TagConverter : IArgumentConverter<TagModel>
    {
        public async Task<Optional<TagModel>> ConvertAsync(string value, CommandContext context)
        {
            using DatabaseContext database = context.Services.GetRequiredService<DatabaseContext>();
            // We call AsNoTracking due to the converter and the command having separate instances of DatabaseContext.
            TagModel? tag = await database.Tags.AsNoTracking().FirstOrDefaultAsync(tag => tag.GuildId == context.Guild.Id && (tag.Name == value || tag.Aliases.Contains(value)));
            if (tag == null)
            {
                await context.RespondAsync(Formatter.Bold($"[Error]: Tag {Formatter.InlineCode(value.ToLowerInvariant())} not found."));
                return Optional.FromNoValue<TagModel>();
            }

            return Optional.FromValue(tag);
        }
    }
}