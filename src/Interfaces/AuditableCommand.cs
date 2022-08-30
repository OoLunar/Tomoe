using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Interfaces
{
    public abstract class AuditableCommand : BaseCommandModule
    {
        public AuditModel Audit { get; private set; } = null!;
        private AuditService AuditService { get; set; } = null!;

        public override async Task BeforeExecutionAsync(CommandContext context)
        {
            AuditService = context.Services.GetRequiredService<AuditService>();
            GuildModel? guildModel = await context.Services.GetRequiredService<GuildModelResolver>().GetAsync(context.Guild.Id);
            if (guildModel == null)
            {
                throw new InvalidOperationException($"Guild {context.Guild.Id} does not exist in the database yet!");
            }

            Audit = new AuditModel(guildModel, context.Member!);
            await base.BeforeExecutionAsync(context);
        }

        public override Task AfterExecutionAsync(CommandContext context) => AuditService.AddAsync(this, context.Guild.Id);
    }
}
