using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("strike"), Description("Gives a strike/warning to the specified victim."), RequireUserPermissions(Permissions.KickMembers), Aliases("warn"), Punishment]
	public class Strikes : BaseCommandModule
	{
		[GroupCommand]
		public async Task Add(CommandContext context, DiscordUser victim, [RemainingText] string strikeReason = Constants.MissingReason)
		{
			Strike strike = new();
			strike.CreatedAt = DateTime.UtcNow;
			strike.Dropped = false;
			strike.GuildId = context.Guild.Id;
			strike.IssuerId = context.User.Id;
			strike.JumpLink = context.Message.JumpLink;
			strike.Reason.Add(strikeReason.Trim());
			strike.Id = Program.Database.Strikes.Count(strike => strike.GuildId == context.Guild.Id);
			strike.VictimId = victim.Id;
			strike.VictimMessaged = false;

			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been given a strike by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(strikeReason))}");
					strike.VictimMessaged = true;
				}
				catch (UnauthorizedException) { }
			_ = await Program.SendMessage(context, $"Case #{strike}, {victim.Mention} has been striked{(strike.VictimMessaged ? '.' : " (Failed to DM).")} This is strike #{Program.Database.Strikes.Count(strike => strike.VictimId == context.User.Id && !strike.Dropped)}. Reason: {Formatter.BlockCode(Formatter.Strip(strikeReason))}", null, new UserMention(victim.Id));
		}

		[Command("check"), Description("Gets the users past history"), RequireUserPermissions(Permissions.KickMembers), Aliases("history")]
		public async Task Check(CommandContext context, DiscordUser victim)
		{
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"{victim.Username}'s Past History");
			Strike[] pastStrikes = await Program.Database.Strikes.Where(strike => strike.GuildId == context.Guild.Id && strike.VictimId == victim.Id).ToArrayAsync();
			if (pastStrikes == null) _ = await Program.SendMessage(context, "No previous strikes have been found!");
			else
			{
				foreach (Strike strike in pastStrikes) embedBuilder.Description += $"Case #{strike.Id} [on {strike.CreatedAt.ToString("MMM' 'dd', 'yyyy' 'HH':'mm':'ss", CultureInfo.InvariantCulture)}, Issued by {(await context.Client.GetUserAsync(strike.IssuerId)).Mention}]({strike.JumpLink}) {(strike.Dropped ? "(Dropped)" : null)}\n";
				_ = await Program.SendMessage(context, null, embedBuilder.Build());
			}
		}
	}
}
