using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Interfaces
{
    /// <summary>
    /// Used as a base for commands that require to be audited.
    /// </summary>
    public abstract class AuditableCommand : BaseCommandModule
    {
        /// <summary>
        /// The audit to fill out.
        /// </summary>
        public AuditModel Audit { get; private set; } = null!;
        private AuditService AuditService { get; set; } = null!;

        /// <summary>
        /// Set the audit before the command is executed.
        /// </summary>
        public override async Task BeforeExecutionAsync(CommandContext context)
        {
            AuditService = context.Services.GetRequiredService<AuditService>();

            // Get the guild model from the resolver service, ensuring that audit commands can be used.
            GuildModel? guildModel = await context.Services.GetRequiredService<GuildModelResolverService>().GetAsync(context.Guild.Id);
            if (guildModel == null)
            {
                throw new InvalidOperationException($"Guild {context.Guild.Id} does not exist in the database yet!");
            }

            Audit = new AuditModel(guildModel, context.Member!);
            await base.BeforeExecutionAsync(context);
        }

        /// <summary>
        /// Add the audit log to the database after the command was successfully executed.
        /// </summary>
        public override Task AfterExecutionAsync(CommandContext context) => AuditService.AddAsync(this, context.Guild.Id);
    }
}
