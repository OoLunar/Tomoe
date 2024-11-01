using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.AutoCompleteProviders
{
    public sealed class SharedGuildsAutoCompleteProvider : IAutoCompleteProvider
    {
        public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
        {
            IReadOnlyList<ulong> sharedGuilds = await GuildMemberModel.FindMutualGuildsAsync(context.User.Id);
            List<DiscordAutoCompleteChoice> choices = [];
            foreach (ulong guildId in sharedGuilds)
            {
                if (!context.Client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
                {
                    continue;
                }
                else if (!string.IsNullOrWhiteSpace(context.UserInput) && !guild.Name.Contains(context.UserInput, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                choices.Add(new DiscordAutoCompleteChoice(guild.Name, guildId.ToString(CultureInfo.InvariantCulture)));
                if (choices.Count >= 25)
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(context.UserInput))
            {
                // Sort alphabetically.
                choices.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                // Sort by if the name starts with the user input, then alphabetically.
                choices.Sort((a, b) =>
                    a.Name.StartsWith(context.UserInput, StringComparison.InvariantCultureIgnoreCase) == b.Name.StartsWith(context.UserInput, StringComparison.InvariantCultureIgnoreCase)
                        ? string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase)
                        : a.Name.StartsWith(context.UserInput, StringComparison.InvariantCultureIgnoreCase) ? -1 : 1);
            }

            return choices;
        }
    }
}
