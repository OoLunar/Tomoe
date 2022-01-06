using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Api
{
    public partial class Moderation
    {
        public class Strikes
        {
            public static async Task<bool> Create(DiscordGuild discordGuild, ulong victimId, ulong issuerId, string discordMessageLink, string strikeReason)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();

                Strike strike = new();
                strike.GuildId = discordGuild.Id;
                strike.IssuerId = issuerId;
                strike.JumpLinks.Add(discordMessageLink);
                strike.Reasons.Add(strikeReason);
                strike.VictimId = victimId;
                strike.LogId = database.Strikes.Where(strike => strike.GuildId == discordGuild.Id).Count() + 1;
                strike.VictimMessaged = await (await victimId.GetMember(discordGuild)).TryDmMember($"You've been given a strike by <@{issuerId}> from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(strikeReason))}Context: {discordMessageLink}");
                database.Strikes.Add(strike);
                await ModLog(discordGuild, LogType.Strike, database, $"<@{issuerId}> striked <@{victimId}>{(strike.VictimMessaged ? '.' : "(failed to dm.)")} Reason: {strikeReason}");
                await database.SaveChangesAsync();

                return strike.VictimMessaged;
            }

            public static async Task<bool> Drop(DiscordGuild discordGuild, Strike strike, ulong issuerId, string discordMessageLink, string dropReason)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();

                strike.VictimMessaged = await (await strike.VictimId.GetMember(discordGuild)).TryDmMember($"Strike #{strike.LogId} has been dropped by <@{issuerId}> from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(dropReason))}Context: {discordMessageLink}");
                strike.JumpLinks.Add(discordMessageLink);
                strike.Reasons.Add("Drop Reason: " + dropReason);
                strike.Dropped = true;
                database.Entry(strike).State = EntityState.Modified;
                await ModLog(discordGuild, LogType.Pardon, database, $"<@{issuerId}> dropped <@{strike.VictimId}>'s strike #{strike.LogId}{(strike.VictimMessaged ? '.' : "(failed to dm).")} Reason: {dropReason}");
                await database.SaveChangesAsync();

                return strike.VictimMessaged;
            }

            public static async Task<bool> Restrike(DiscordGuild discordGuild, Strike strike, ulong issuerId, string discordMessageLink, string restrikeReason)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();

                strike.VictimMessaged = await (await strike.VictimId.GetMember(discordGuild)).TryDmMember($"Strike #{strike.LogId} has been reapplied by <@{issuerId}> from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(restrikeReason))}Context: {discordMessageLink}");
                strike.JumpLinks.Add(discordMessageLink);
                strike.Reasons.Add("Restrike Reason: " + restrikeReason);
                strike.Dropped = false;
                database.Entry(strike).State = EntityState.Modified;
                await ModLog(discordGuild, LogType.Restrike, database, $"<@{issuerId}> has reapplied <@{strike.VictimId}>'s strike #{strike.LogId}{(strike.VictimMessaged ? '.' : "(failed to dm).")} Reason: {restrikeReason}");
                await database.SaveChangesAsync();

                return strike.VictimMessaged;
            }

            public static Strike GetStrike(int strikeId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.Strikes.FirstOrDefault(strike => strike.Id == strikeId);
            }

            public static Strike GetGuildStrike(ulong discordGuildId, int strikeId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.Strikes.FirstOrDefault(strike => strike.GuildId == discordGuildId && strike.LogId == strikeId);
            }

            public static List<Strike> GetStrikes(ulong discordGuildId, ulong victimId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.Strikes.Where(strike => strike.GuildId == discordGuildId && strike.VictimId == victimId).OrderBy(strike => strike.LogId).ToList();
            }

            public static List<Strike> GetStrikes(ulong victimId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.Strikes.Where(strike => strike.VictimId == victimId).OrderBy(strike => strike.Id).ToList();
            }

            public static List<Strike> GetIssued(ulong discordGuildId, ulong issuerId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.Strikes.Where(strike => strike.GuildId == discordGuildId && strike.IssuerId == issuerId).OrderBy(strike => strike.LogId).ToList();
            }

            public static List<Strike> GetIssued(ulong issuerId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.Strikes.Where(strike => strike.IssuerId == issuerId).OrderBy(strike => strike.Id).ToList();
            }
        }
    }
}