using System;
using System.Linq;
using System.Threading.Tasks;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.Tomoe.Database;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class MemberCountCommand : BaseCommand
    {
        private readonly DatabaseContext _databaseContext;

        public MemberCountCommand(DatabaseContext databaseContext) => _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));

        [Command("member_count")]
        public async Task ExecuteAsync(CommandContext context)
        {
            if (context.Guild is null)
            {
                await context.ReplyAsync($"Command `/{context.CurrentCommand.FullName}` can only be used in a guild.");
                return;
            }

            await context.ReplyAsync($"Current member count: {_databaseContext.Members.Count(member => member.GuildId == context.Guild.Id):N0}");
        }
    }
}
