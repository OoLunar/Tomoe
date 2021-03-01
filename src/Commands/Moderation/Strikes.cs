using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("strike"), Description("Gives a strike/warning to the specified victim."), RequireUserPermissions(Permissions.KickMembers), Aliases("warn", "add"), Punishment]
	public class Strikes : BaseCommandModule
	{
		public Database Database { private get; set; }

		[GroupCommand]
		public async Task Add(CommandContext context, DiscordUser victim, [RemainingText] string strikeReason = Constants.MissingReason)
		{
			Strike strike = new(context.Guild.Id);
			strike.CreatedAt = DateTime.UtcNow;
			strike.Dropped = false;
			strike.IssuerId = context.User.Id;
			strike.JumpLink = context.Message.JumpLink;
			strike.Reasons.Add(strikeReason.Trim());
			strike.VictimId = victim.Id;
			strike.VictimMessaged = false;
			_ = Database.Strikes.Add(strike);
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been given a strike by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(strikeReason))}");
					strike.VictimMessaged = true;
				}
				catch (UnauthorizedException) { }
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Case #{strike.Id}, {victim.Mention} has been striked{(strike.VictimMessaged ? '.' : " (Failed to DM).")} This is strike #{Database.Strikes.Count(strike => strike.VictimId == context.User.Id && !strike.Dropped) + 1}. Reason: {Formatter.BlockCode(Formatter.Strip(strikeReason))}", null, new UserMention(victim.Id));
		}

		[Command("check"), Description("Gets the users past history"), RequireUserPermissions(Permissions.KickMembers), Aliases("history", "list")]
		public async Task Check(CommandContext context, DiscordUser victim)
		{
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context);
			embedBuilder.Title = $"{victim.Username}'s Past History";
			embedBuilder.Author = new()
			{
				Name = victim.Username,
				Url = victim.AvatarUrl,
				IconUrl = victim.AvatarUrl
			};

			Strike[] pastStrikes = await Database.Strikes.Where(strike => strike.GuildId == context.Guild.Id && strike.VictimId == victim.Id).ToArrayAsync();
			if (pastStrikes.Length == 0) _ = await Program.SendMessage(context, "No previous strikes have been found!");
			else
			{
				foreach (Strike strike in pastStrikes) embedBuilder.Description += $"Case #{strike.Id} [on {strike.CreatedAt.ToString("MMM' 'dd', 'yyyy' 'HH':'mm':'ss", CultureInfo.InvariantCulture)}, Issued by {(await context.Client.GetUserAsync(strike.IssuerId)).Mention}]({strike.JumpLink}) {(strike.Dropped ? "(Dropped)" : null)}\n";
				_ = await Program.SendMessage(context, null, embedBuilder.Build());
			}
		}

		[Command("drop"), Description("Drops a strike."), Punishment, Aliases("pardon")]
		public async Task Drop(CommandContext context, Strike strike, [RemainingText] string pardonReason = Constants.MissingReason)
		{
			if (strike.Dropped)
			{
				_ = await Program.SendMessage(context, $"Strike #{strike.Id} has already been pardoned!");
				return;
			}
			strike.Dropped = true;
			strike.Reasons.Add("Drop Reason: " + pardonReason.Trim());
			strike.VictimMessaged = false;

			DiscordMember guildVictim = (await context.Client.GetUserAsync(strike.VictimId)).GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"Strike #{strike.Id} has been dropped by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}\nContext: {strike.JumpLink}");
					strike.VictimMessaged = true;
				}
				catch (UnauthorizedException) { }
			Database.Entry(strike).State = EntityState.Modified;
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Case #{strike.Id} has been dropped, <@{strike.VictimId}> has been pardoned{(strike.Dropped ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}", null, new UserMention(strike.VictimId));
		}

		[Command("info"), Description("Gives info about a strike."), Punishment, Aliases("lookup")]
		public async Task Info(CommandContext context, Strike strike)
		{
			DiscordUser victim = await context.Client.GetUserAsync(strike.VictimId);
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context);
			embedBuilder.Title = $"Case #{strike.Id}";
			embedBuilder.Author = new()
			{
				Name = victim.Username,
				IconUrl = victim.AvatarUrl,
				Url = victim.AvatarUrl
			};
			embedBuilder.Description += $"Issued At: {strike.CreatedAt}\n";
			embedBuilder.Description += $"Issued By: <@{strike.IssuerId}>\n";
			embedBuilder.Description += $"Victim: <@{strike.VictimId}>\n";
			embedBuilder.Description += $"Victim Messaged: {strike.VictimMessaged}\n";
			embedBuilder.Description += $"Dropped: {(strike.Dropped ? "Yes" : "No")}\n";
			embedBuilder.Description += Formatter.MaskedUrl("Jumplink", strike.JumpLink, strike.JumpLink.ToString()) + '\n';
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			List<Page> pages = new();
			for (int i = 0; i < strike.Reasons.Count; i++)
			{
				if (i != 0 && (i % 25) == 0)
				{
					pages.Add(new(null, embedBuilder));
					_ = embedBuilder.ClearFields();
				}
				_ = embedBuilder.AddField(i == 0 ? $"Reason 1 (Original)" : $"Reason {i + 1}", strike.Reasons[i], true);
			}
			if (pages.Count == 0) _ = await Program.SendMessage(context, null, embedBuilder);
			else
			{
				pages.Add(new(null, embedBuilder));
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
		}

		public static async Task Automated(DiscordGuild discordGuild, Database database, DiscordUser victim, Uri jumplink, string strikeReason = Constants.MissingReason)
		{
			Strike strike = new(discordGuild.Id);
			strike.CreatedAt = DateTime.UtcNow;
			strike.Dropped = false;
			strike.IssuerId = Program.Client.CurrentUser.Id;
			strike.JumpLink = jumplink;
			strike.Reasons.Add(strikeReason.Trim());
			strike.VictimId = victim.Id;
			strike.VictimMessaged = false;
			_ = database.Strikes.Add(strike);
			DiscordMember guildVictim = victim.GetMember(discordGuild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been given a strike by {Formatter.Bold(Program.Client.CurrentUser.Mention)} from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(strikeReason))}\nContext: {Formatter.EmbedlessUrl(strike.JumpLink)}");
					strike.VictimMessaged = true;
				}
				catch (UnauthorizedException) { }
			_ = await database.SaveChangesAsync();
		}
	}
}
