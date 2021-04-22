using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("strike"), RequireGuild, Punishment(false)]
	public class Strikes : BaseCommandModule
	{
		public Database Database { private get; set; }

		[GroupCommand, RequireUserPermissions(Permissions.KickMembers), Description("Adds a strike to the victim.")]
		public async Task ByUser(CommandContext context, DiscordUser victim, [RemainingText] string muteReason = Constants.MissingReason)
		{
			bool sentDm = await ByProgram(context.Guild, victim, context.User.Id, context.Message.JumpLink, muteReason);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been striked{(sentDm ? '.' : " (Failed to dm).")}");
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

			Strike[] pastStrikes = await Database.Strikes.Where(strike => strike.GuildId == context.Guild.Id && strike.VictimId == victim.Id).OrderBy(strike => strike.Id).ToArrayAsync();
			if (pastStrikes.Length == 0) _ = await Program.SendMessage(context, "No previous strikes have been found!");
			else
			{
				foreach (Strike strike in pastStrikes) embedBuilder.Description += $"Case #{strike.Id} [on {strike.CreatedAt.ToString("MMM' 'dd', 'yyyy' 'HH':'mm':'ss", CultureInfo.InvariantCulture)}, Issued by {(await context.Client.GetUserAsync(strike.IssuerId)).Mention}]({strike.JumpLinks.First()}) {(strike.Dropped ? "(Dropped)" : null)}\n";
				_ = await Program.SendMessage(context, null, embedBuilder.Build());
			}
		}

		[Command("drop"), Description("Drops a strike."), Aliases("pardon")]
		public async Task Drop(CommandContext context, Strike strike, [RemainingText] string pardonReason = Constants.MissingReason)
		{
			// Attach because strike gets detached when handed off from converter
			_ = Database.Strikes.Attach(strike);
			if (strike.Dropped)
			{
				_ = await Program.SendMessage(context, $"Strike #{strike.Id} has already been pardoned!");
				return;
			}
			strike.Dropped = true;
			strike.Reasons.Add("Drop Reason: " + pardonReason.Trim());
			strike.JumpLinks.Add(context.Message.JumpLink);
			strike.VictimMessaged = false;

			DiscordMember guildVictim = await strike.VictimId.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot)
			{
				try
				{
					_ = await guildVictim.SendMessageAsync($"Strike #{strike.Id} has been dropped from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}Context: {context.Message.JumpLink}");
					strike.VictimMessaged = true;
				}
				catch (Exception) { }
			}

			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Case #{strike.Id} has been dropped, <@{strike.VictimId}> has been pardoned{(strike.Dropped ? '.' : " (Failed to DM).")}", null, new UserMention(strike.VictimId));
		}

		[Command("drop")]
		public async Task Drop(CommandContext context, DiscordUser discordUser, [RemainingText] string pardonReason = Constants.MissingReason)
		{
			Strike strike = await Database.Strikes.LastOrDefaultAsync(strike => strike.VictimId == discordUser.Id && !strike.Dropped);
			if (strike == null)
			{
				_ = await Program.SendMessage(context, $"**[Error: {discordUser.Mention} has no strikes that can be dropped!]**");
				return;
			}
			strike.Dropped = true;
			strike.Reasons.Add("Drop Reason: " + pardonReason.Trim());
			strike.JumpLinks.Add(context.Message.JumpLink);
			strike.VictimMessaged = false;

			DiscordMember guildVictim = await strike.VictimId.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot)
			{
				try
				{
					_ = await guildVictim.SendMessageAsync($"Strike #{strike.Id} has been dropped from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(pardonReason))}Context: {context.Message.JumpLink}");
					strike.VictimMessaged = true;
				}
				catch (Exception) { }
			}

			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Case #{strike.Id} has been dropped, <@{strike.VictimId}> has been pardoned{(strike.Dropped ? '.' : " (Failed to DM).")}", null, new UserMention(strike.VictimId));
		}

		[Command("info"), Description("Gives info about a strike."), Punishment, Aliases("lookup")]
		public async Task Info(CommandContext context, Strike strike)
		{
			DiscordUser victim = await context.Client.GetUserAsync(strike.VictimId);
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context);
			embedBuilder.Title = $"Case #{strike.Id}";
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
					_ = embedBuilder.ClearFields();
				}
				_ = embedBuilder.AddField(i == 0 ? $"Reason 1 (Original)" : $"Reason {i + 1}", Formatter.MaskedUrl(strike.Reasons[i], strike.JumpLinks[i]), true);
			}
			if (pages.Count == 0) _ = await Program.SendMessage(context, null, embedBuilder);
			else
			{
				pages.Add(new(null, embedBuilder));
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
		}

		public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, ulong issuerId, Uri jumplink, string muteReason = Constants.MissingPermissions)
		{
			DiscordMember guildVictim = await victim.Id.GetMember(discordGuild);

			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			GuildConfig guildConfig = await database.GuildConfigs.Where(guildConfig => guildConfig.Id == discordGuild.Id).DefaultIfEmpty(new GuildConfig(discordGuild.Id)).SingleAsync();
			GuildUser databaseVictim = await database.GuildUsers.Where(guildUser => guildUser.UserId == victim.Id && guildUser.GuildId == discordGuild.Id).DefaultIfEmpty(new GuildUser(victim.Id)).SingleAsync();

			// If the user is in the guild, assign the muted role
			bool sentDm = false;
			if (guildVictim != null && !guildVictim.IsBot)
			{
				try
				{
					_ = await guildVictim.SendMessageAsync($"You've been given a strike from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}Context: {jumplink}");
					sentDm = true;
				}
				catch (Exception) { }
			}

			Strike strike = new();
			strike.GuildId = discordGuild.Id;
			strike.IssuerId = issuerId;
			strike.JumpLinks.Add(jumplink);
			strike.Reasons.Add(muteReason);
			strike.VictimId = victim.Id;
			strike.VictimMessaged = sentDm;
			_ = database.Strikes.Add(strike);
			_ = await database.SaveChangesAsync();

			return sentDm;
		}

		public static async Task ProgressiveStrike(DiscordGuild discordGuild, DiscordUser victim, Strike strike)
		{
			//using IServiceScope scope = Program.ServiceProvider.CreateScope();
			//Database database = scope.ServiceProvider.GetService<Database>();
			//
			//Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
			//if (guild == null) return;
			//
			//int totalStrikeCount = await database.Strikes.CountAsync(strike => strike.VictimId == victim.Id);
			//if (!guild.Punishments.TryGetValue(totalStrikeCount, out ProgressiveStrike progressiveStrike)) return;
			//switch (progressiveStrike.Punishment)
			//{
			//	case Moderation.ProgressiveStrike.PunishmentOption.Ban:
			//		await Ban.ByProgram(discordGuild, victim, strike.JumpLinks.Last(), $"Reached progressive strike #{totalStrikeCount}");
			//		break;
			//	case Moderation.ProgressiveStrike.PunishmentOption.Kick:
			//		await Kick.ByProgram(discordGuild, await victim.Id.GetMember(discordGuild), strike.JumpLinks.Last(), $"Reached progressive strike #{totalStrikeCount}");
			//		break;
			//	case Moderation.ProgressiveStrike.PunishmentOption.Mute:
			//	case Moderation.ProgressiveStrike.PunishmentOption.Antimeme:
			//	case Moderation.ProgressiveStrike.PunishmentOption.Voiceban:
			//		//await Voiceban.ByProgram(discordGuild, victim, strike.JumpLinks.Last(), "");
			//		break;
			//	default: throw new NotImplementedException();
			//}
		}
	}

	public class ProgressiveStrike
	{
		public enum PunishmentOption
		{
			Ban,
			Tempban,
			Kick,
			Mute,
			Tempmute,
			Antimeme,
			Tempantimeme,
			Voiceban,
			Tempvoiceban
		}

		public PunishmentOption Punishment { get; private set; }
		public TimeSpan TimeSpan { get; private set; }
	}
}
