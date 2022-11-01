using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    public sealed partial class StrikeCommand : ApplicationCommandModule
    {
        [SlashCommand("info", "Gets information on a strike.")]
        public async Task InfoAsync(InteractionContext context, [Option("strike_id", "Which strike to get information on.")] long strikeId)
        {
            Strike strike = Database.Strikes.FirstOrDefault(databaseStrike => databaseStrike.LogId == strikeId && databaseStrike.GuildId == context.Guild.Id);
            string embedDescription = $"Created At: {strike.Changes.First().Humanize()}";
            embedDescription += $"Issued By: <@{strike.IssuerId}>";
            embedDescription += $"Victim: <@{strike.VictimId}>";
            embedDescription += "Victim Messaged: " + (strike.VictimMessaged ? "Yes" : "No");
            embedDescription += "Dropped: " + (strike.Dropped ? "Yes" : "No");

            DiscordUser victim = await context.Client.GetUserAsync(strike.VictimId);
            DiscordEmbedBuilder embedBuilder = null;

            List<DiscordEmbed> embeds = new();
            for (int i = 0; i < strike.Reasons.Count; i++)
            {
                if (i == 0 || (i % 25) == 0)
                {
                    embeds.Add(embedBuilder);
                    embedBuilder = new()
                    {
                        Title = $"Strike #{strikeId}, Page {i + 1}",
                        Description = embedDescription,
                        Color = new DiscordColor("#7b84d1"),
                        Author = new()
                        {
                            Name = victim.Username,
                            IconUrl = victim.AvatarUrl,
                            Url = victim.AvatarUrl
                        }
                    };
                }

                embedBuilder.AddField("Reason " + (i + 1), strike.Reasons[i], true);
            }
            await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(embeds));
        }
    }
}
