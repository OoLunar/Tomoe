namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Moderation.Attributes;
    using Tomoe.Db;

    [Group("strike"), RequireGuild, Description("Assigns a strike to a specific individual."), Aliases("warn"), RequireUserPermissions(Permissions.KickMembers)]
    public class Strikes : BaseCommandModule
    {
        public Database Database { private get; set; }

        [GroupCommand]
        public async Task ByUser(CommandContext context, [Description("Who's being striked.")] DiscordUser victim, [Description("Why is the victim being striked."), RemainingText] string strikeReason = Constants.MissingReason)
        {
            // CommandHandler will handle the HierarchyException that ExecuteCheckAsync throws.
            if (await new Punishment(false).ExecuteCheckAsync(context, false))
            {
                await Program.SendMessage(context, $"{victim.Mention} has been striked{(await Api.Moderation.Strikes.Create(context.Guild, victim.Id, context.User.Id, context.Message.JumpLink.ToString(), strikeReason) ? '.' : "(failed to dm.)")}");
            }
        }

        [Command("drop"), Description("Drops a strike from the user's record."), Aliases("pardon", "remove")]
        public async Task Drop(CommandContext context, [Description("The strike to drop.")] Strike strike, [Description("Why is the strike being dropped."), RemainingText] string dropReason = Constants.MissingReason)
        {
            if (strike.Dropped)
            {
                await Program.SendMessage(context, $"Strike #{strike.LogId} is already dropped!");
                return;
            }

            if (!await new Punishment(false).ExecuteCheckAsync(context, false))
            {
                return;
            }
            await Program.SendMessage(context, $"Strike #{strike.LogId} has been dropped{(await Api.Moderation.Strikes.Drop(context.Guild, strike, context.User.Id, context.Message.JumpLink.ToString(), dropReason) ? '.' : "(failed to dm.)")}");
            Database.Entry(strike).State = EntityState.Detached;
        }

        [Command("drop")]
        public async Task Drop(CommandContext context, [Description("Who to get the last strike from.")] DiscordUser victim, [Description("Why is the strike being dropped."), RemainingText] string dropReason = Constants.MissingReason)
        {
            Strike strike = Database.Strikes.AsNoTracking().OrderBy(strike => strike.Id).LastOrDefault(strike => strike.VictimId == victim.Id && strike.GuildId == context.Guild.Id);
            if (strike == null)
            {
                await Program.SendMessage(context, $"{victim.Mention} doesn't have any strikes!");
            }
            else
            {
                await Drop(context, strike, dropReason);
            }
        }

        [Command("info"), Description("Gives info about a strike."), Aliases("lookup")]
        public async Task Info(CommandContext context, [Description("Which strike to get information on.")] Strike strike)
        {
            DiscordUser victim = await context.Client.GetUserAsync(strike.VictimId);
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context);
            embedBuilder.Title = $"Case #{strike.LogId}";
            embedBuilder.Description += $"Issued At: {strike.CreatedAt}\n";
            embedBuilder.Description += $"Issued By: <@{strike.IssuerId}>\n";
            embedBuilder.Description += $"Victim: <@{strike.VictimId}>\n";
            embedBuilder.Description += $"Victim Messaged: {strike.VictimMessaged}\n";
            embedBuilder.Description += $"Dropped: {(strike.Dropped ? "Yes" : "No")}\n";
            embedBuilder.Author = new()
            {
                Name = victim.Username,
                IconUrl = victim.AvatarUrl,
                Url = victim.AvatarUrl
            };
            InteractivityExtension interactivity = context.Client.GetInteractivity();
            List<Page> pages = new();
            for (int i = 0; i < strike.Reasons.Count; i++)
            {
                if (i != 0 && (i % 25) == 0)
                {
                    pages.Add(new(null, embedBuilder));
                    embedBuilder.ClearFields();
                }
                embedBuilder.AddField(i == 0 ? $"Reason 1 (Original)" : $"Reason {i + 1}", Formatter.MaskedUrl(strike.Reasons[i], new Uri(strike.JumpLinks[i])), true);
            }
            if (pages.Count == 0)
            {
                await Program.SendMessage(context, null, embedBuilder);
            }
            else
            {
                pages.Add(new(null, embedBuilder));
                await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
            }
        }

        [Command("check"), Description("Gets the users past history"), Aliases("history", "list")]
        public async Task Check(CommandContext context, [Description("The victim to get the history on.")] DiscordUser victim)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context);
            embedBuilder.Title = $"{victim.Username}'s Past History";
            embedBuilder.Author = new()
            {
                Name = victim.Username,
                Url = victim.AvatarUrl,
                IconUrl = victim.AvatarUrl
            };

            List<Strike> pastStrikes = Api.Moderation.Strikes.GetStrikes(context.Guild.Id, victim.Id);
            if (pastStrikes.Count == 0)
            {
                await Program.SendMessage(context, "No previous strikes have been found!");
            }
            else
            {
                foreach (Strike strike in pastStrikes)
                {
                    embedBuilder.Description += $"Case #{strike.LogId} [on {strike.CreatedAt.ToString("MMM' 'dd', 'yyyy' 'HH':'mm':'ss", CultureInfo.InvariantCulture)}, Issued by {(await context.Client.GetUserAsync(strike.IssuerId)).Mention}]({strike.JumpLinks.First()}) {(strike.Dropped ? "(Dropped)" : null)}\n";
                }

                await Program.SendMessage(context, null, embedBuilder.Build());
            }
        }

        [Command("restrike"), Description("Turns a dropped strike into a normal strike."), Aliases("reapply", "undrop")]
        public async Task Restrike(CommandContext context, [Description("Which strike to restrike.")] Strike strike, [Description("Why is the strike being reapplied?"), RemainingText] string restrikeReason = Constants.MissingReason)
        {
            if (!strike.Dropped)
            {
                await Program.SendMessage(context, $"Strike #{strike.LogId} isn't dropped!");
                return;
            }

            if (!await new Punishment(false).ExecuteCheckAsync(context, false))
            {
                return;
            }

            await Program.SendMessage(context, $"Strike #{strike.LogId} has been reapplied{(await Api.Moderation.Strikes.Restrike(context.Guild, strike, context.User.Id, context.Message.JumpLink.ToString(), restrikeReason) ? '.' : "(failed to dm.)")}");
            Database.Entry(strike).State = EntityState.Detached;
        }
    }
}
