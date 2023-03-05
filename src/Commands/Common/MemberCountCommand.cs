using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class MemberCountCommand : BaseCommand
    {
        private readonly DatabaseContext _databaseContext;

        public MemberCountCommand(DatabaseContext databaseContext) => _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));

        [Command("member_count", "mc")]
        public Task ExecuteAsync(CommandContext context) => context.Guild is null
            ? context.ReplyAsync($"Command `/{context.CurrentCommand.FullName}` can only be used in a guild.")
            : context.ReplyAsync($"Current member count: {_databaseContext.Members.Count(member => member.GuildId == context.Guild.Id && !member.Flags.HasFlag(MemberState.Absent | MemberState.Banned)):N0}");
    }
}
