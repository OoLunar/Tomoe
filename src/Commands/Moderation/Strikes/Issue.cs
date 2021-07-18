namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommandGroup("strike", "Handles warnings or strikes for an individual.")]
        public partial class Strikes : SlashCommandModule
        {
            public Database Database { private get; set; }

            [SlashCommand("issue", "Creates a new strike for an individual."), Hierarchy(Permissions.KickMembers)]
            public async Task Issue(InteractionContext context, [Option("victim", "Who is being striked?")] DiscordUser victim, [Option("reason", "Why is the user being striked?")] string reason = Constants.MissingReason)
            {
                Strike strike = new()
                {
                    LogId = Database.Strikes.Count(databaseStrike => databaseStrike.GuildId == context.Guild.Id) + 1,
                    GuildId = context.Guild.Id,
                    IssuerId = context.User.Id,
                    VictimId = victim.Id
                };

                DiscordMember guildVictim = await victim.Id.GetMember(context.Guild);
                bool sentDm = await guildVictim.TryDmMember($"{context.Member.Mention} ({context.Member.Username}#{context.Member.Discriminator}) gave you a strike.\nReason: {Formatter.BlockCode(Formatter.Strip(reason))}");

                strike.VictimMessaged = sentDm;
                strike.Reasons.Add(reason);
                Database.Strikes.Add(strike);
                await Database.SaveChangesAsync();

                Dictionary<string, string> keyValuePairs = new();
                keyValuePairs.Add("guild_name", context.Guild.Name);
                keyValuePairs.Add("guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric());
                keyValuePairs.Add("guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture));
                keyValuePairs.Add("victim_username", guildVictim.Username);
                keyValuePairs.Add("victim_tag", guildVictim.Discriminator);
                keyValuePairs.Add("victim_mention", guildVictim.Mention);
                keyValuePairs.Add("victim_id", guildVictim.Id.ToString(CultureInfo.InvariantCulture));
                keyValuePairs.Add("victim_displayname", guildVictim.DisplayName);
                keyValuePairs.Add("moderator_username", context.Member.Username);
                keyValuePairs.Add("moderator_tag", context.Member.Discriminator);
                keyValuePairs.Add("moderator_mention", context.Member.Mention);
                keyValuePairs.Add("moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture));
                keyValuePairs.Add("moderator_displayname", context.Member.DisplayName);
                keyValuePairs.Add("punishment_reason", reason);
                keyValuePairs.Add("strike_id", strike.LogId.ToString(CultureInfo.InvariantCulture));
                await ModLog(context.Guild, keyValuePairs, CustomEvent.Drop, Database);

                await context.EditResponseAsync(new()
                {
                    Content = $"{victim.Mention} has been striked{(sentDm ? "" : "(failed to dm)")}. Reason: {reason}"
                });
            }
        }
    }
}