using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Commands.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands
{
    public partial class Moderation : ApplicationCommandModule
    {
        public partial class Strikes : ApplicationCommandModule
        {
            [SlashCommand("drop", "Drops a previously issued strike for an individual."), Hierarchy(Permissions.KickMembers)]
            public async Task DropAsync(InteractionContext context, [Option("strike_id", "Which strike to drop.")] long strikeId, [Option("reason", "Why is the strike being dropped?")] string reason = Constants.MissingReason)
            {
                Strike strike = Database.Strikes.FirstOrDefault(databaseStrike => databaseStrike.LogId == strikeId && databaseStrike.GuildId == context.Guild.Id);
                if (strike.Dropped)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: Strike #{strikeId} is already dropped!"
                    });
                }

                DiscordMember guildVictim = await strike.VictimId.GetMemberAsync(context.Guild);
                bool sentDm = await guildVictim.TryDmMemberAsync($"{context.Member.Mention} ({context.Member.Username}#{context.Member.Discriminator}) dropped strike #{strikeId}.\nReason: {Formatter.BlockCode(Formatter.Strip(reason))}");

                strike.VictimMessaged = sentDm;
                strike.Reasons.Add("Dropped: " + reason);
                strike.Changes.Add(DateTime.UtcNow);
                strike.Dropped = true;

                Dictionary<string, string> keyValuePairs = new()
                {
                    { "guild_name", context.Guild.Name },
                    { "guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric() },
                    { "guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture) },
                    { "victim_username", guildVictim.Username },
                    { "victim_tag", guildVictim.Discriminator },
                    { "victim_mention", guildVictim.Mention },
                    { "victim_id", guildVictim.Id.ToString(CultureInfo.InvariantCulture) },
                    { "victim_displayname", guildVictim.DisplayName },
                    { "moderator_username", context.Member.Username },
                    { "moderator_tag", context.Member.Discriminator },
                    { "moderator_mention", context.Member.Mention },
                    { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                    { "moderator_displayname", context.Member.DisplayName },
                    { "punishment_reason", reason },
                    { "strike_id", strike.LogId.ToString(CultureInfo.InvariantCulture) }
                };
                await ModLogAsync(context.Guild, keyValuePairs, CustomEvent.Drop, Database);

                await context.EditResponseAsync(new()
                {
                    Content = $"Strike #{strikeId} has been dropped, <@{strike.VictimId}> {(sentDm ? "has been notified" : "could not be messaged")}. Reason: {reason}"
                });
            }
        }
    }
}
