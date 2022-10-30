using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    public sealed partial class Strikes : ApplicationCommandModule
    {
        [SlashCommand("history", "Gets information on a user.")]
        public Task HistoryAsync(InteractionContext context, [Option("user", "Who")] DiscordUser victim)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"{victim.Username}'s Past History",
                Color = new DiscordColor("#7b84d1"),
                Author = new()
                {
                    Name = victim.Username,
                    IconUrl = victim.AvatarUrl,
                    Url = victim.AvatarUrl
                }
            };

            List<Strike> pastStrikes = Database.Strikes.Where(databaseStrike => databaseStrike.GuildId == context.Guild.Id && databaseStrike.VictimId == victim.Id).ToList();
            if (pastStrikes.Count == 0)
            {
                return context.EditResponseAsync(new()
                {
                    Content = "No previous strikes have been found!"
                });
            }
            else
            {
                List<DiscordEmbed> embeds = new();
                foreach (Strike strike in pastStrikes)
                {
                    for (int i = 0; i < strike.Reasons.Count; i++)
                    {
                        if (i == 0 || (i % 25) == 0)
                        {
                            embeds.Add(embedBuilder);
                            embedBuilder = new()
                            {
                                Title = $"{victim.Username}'s Past History, Page {i + 1}",
                                Color = new DiscordColor("#7b84d1"),
                                Author = new()
                                {
                                    Name = victim.Username,
                                    IconUrl = victim.AvatarUrl,
                                    Url = victim.AvatarUrl
                                }
                            };
                        }

                        embedBuilder.AddField("Strike # " + strike.Id, $"Issued By: <@{strike.IssuerId}>\nReason:" + strike.Reasons.Last(), true);
                    }
                }

                return context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(embeds));
            }
        }
    }
}
