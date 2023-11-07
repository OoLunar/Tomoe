using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.ContextChecks;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class MemberCountCommand(DatabaseContext databaseContext)
    {
        private readonly DatabaseContext _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));

        [Command("member_count"), RequireGuild]
        public async Task ExecuteAsync(CommandContext context) => await context.RespondAsync($"Current member count: {_databaseContext.Members.Count(member => member.GuildId == context.Guild!.Id && !member.Flags.HasFlag(MemberState.Absent | MemberState.Banned)):N0}");
    }
}
