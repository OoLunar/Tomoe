namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
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
            public async Task Issue(InteractionContext context, [Option("victim", "Who is being striked?")] DiscordUser victim, [Option("reason", "Why is the user being striked?")] string punishReason = Constants.MissingReason)
            {
                await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() { });
                Strike strike = new()
                {
                    LogId = Database.Strikes.Count(databaseStrike => databaseStrike.GuildId == context.Guild.Id) + 1,
                    GuildId = context.Guild.Id,
                    IssuerId = context.User.Id,
                    VictimId = victim.Id
                };

                DiscordMember guildVictim = await victim.Id.GetMember(context.Guild);
                bool sentDm = await guildVictim.TryDmMember($"{context.Member.Mention} ({context.Member.Username}#{context.Member.Discriminator}) gave you a strike.\nReason: {Formatter.BlockCode(Formatter.Strip(punishReason))}");

                strike.VictimMessaged = sentDm;
                strike.Reasons.Add(punishReason);
                Database.Strikes.Add(strike);
                await Database.SaveChangesAsync();

                await context.EditResponseAsync(new()
                {
                    Content = $"{victim.Mention} has been striked{(sentDm ? "" : "(failed to dm)")}. Reason: {punishReason}"
                });
            }
        }
    }
}