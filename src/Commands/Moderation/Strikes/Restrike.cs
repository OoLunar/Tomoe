namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : SlashCommandModule
    {
        public partial class Strikes : SlashCommandModule
        {
            [SlashCommand("reapply", "Reapplies a previously issued strike for an individual."), Hierarchy(Permissions.KickMembers)]
            public async Task Reapply(InteractionContext context, [Option("strike_id", "Which strike to reapply.")] long strikeId, [Option("reason", "Why is the strike being reapplied?")] string punishReason = Constants.MissingReason)
            {
                Strike strike = Database.Strikes.FirstOrDefault(databaseStrike => databaseStrike.LogId == strikeId && databaseStrike.GuildId == context.Guild.Id);
                if (!strike.Dropped)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Strike #{strikeId} has not been dropped!"
                    });
                }

                DiscordMember guildVictim = await strike.VictimId.GetMember(context.Guild);
                bool sentDm = await guildVictim.TryDmMember($"{context.Member.Mention} ({context.Member.Username}#{context.Member.Discriminator}) reapplied strike #{strikeId}.\nReason: {Formatter.BlockCode(Formatter.Strip(punishReason))}");

                strike.VictimMessaged = sentDm;
                strike.Reasons.Add("Reapply: " + punishReason);
                strike.Changes.Add(DateTime.UtcNow);
                strike.Dropped = false;
                await Database.SaveChangesAsync();

                await context.EditResponseAsync(new()
                {
                    Content = $"Strike #{strikeId} has been reapplied, <@{strike.VictimId}> {(sentDm ? "has been notified" : "could not be messaged")}. Reason: {punishReason}"
                });
            }
        }
    }
}