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
        [SlashCommandGroup("strike", "Handles warnings or strikes for an individual.")]
        public partial class Strikes : ApplicationCommandModule
        {
            public Database Database { private get; set; }

            [SlashCommand("issue", "Creates a new strike for an individual."), Hierarchy(Permissions.KickMembers)]
            public async Task IssueAsync(InteractionContext context, [Option("victim", "Who is being striked?")] DiscordUser victim, [Option("reason", "Why is the user being striked?")] string reason = Constants.MissingReason)
            {
                Strike strike = new()
                {
                    LogId = Database.Strikes.Count(databaseStrike => databaseStrike.GuildId == context.Guild.Id) + 1,
                    GuildId = context.Guild.Id,
                    IssuerId = context.User.Id,
                    VictimId = victim.Id
                };

                DiscordMember guildVictim = await victim.Id.GetMemberAsync(context.Guild);
                bool sentDm = await guildVictim.TryDmMemberAsync($"{context.Member.Mention} ({context.Member.Username}#{context.Member.Discriminator}) gave you a strike.\nReason: {Formatter.BlockCode(Formatter.Strip(reason))}");

                strike.VictimMessaged = sentDm;
                strike.Reasons.Add(reason);
                Database.Strikes.Add(strike);
                await Database.SaveChangesAsync();

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
                    Content = $"{victim.Mention} has been striked{(sentDm ? "" : "(failed to dm)")}. Reason: {reason}"
                });
            }
        }
    }
}
