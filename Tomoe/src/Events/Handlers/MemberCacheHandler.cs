using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class MemberCacheHandler
    {
        private readonly IServiceProvider serviceProvider;

        public MemberCacheHandler(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        [DiscordEvent]
        public async Task OnGuildMemberAddAsync(DiscordClient client, GuildMemberAddEventArgs eventArgs)
        {
            DatabaseContext databaseContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            GuildMemberModel? guildMemberModel = databaseContext.Members.FirstOrDefault(member => member.UserId == eventArgs.Member.Id);
            if (guildMemberModel is null)
            {
                guildMemberModel = new GuildMemberModel(eventArgs.Member);
                databaseContext.Members.Add(guildMemberModel);
            }
            else
            {
                // Remove Absent and Banned flags
                guildMemberModel.Flags &= ~(MemberState.Absent | MemberState.Banned);

                // TODO: Assign the user's old roles back to them.
            }
            if (databaseContext.ChangeTracker.HasChanges())
            {
                await databaseContext.SaveChangesAsync();
            }
        }

        [DiscordEvent]
        public async Task OnGuildMemberRemoveAsync(DiscordClient client, GuildMemberRemoveEventArgs eventArgs)
        {
            DatabaseContext databaseContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            GuildMemberModel? guildMemberModel = databaseContext.Members.FirstOrDefault(member => member.UserId == eventArgs.Member.Id);
            if (guildMemberModel is null)
            {
                guildMemberModel = new GuildMemberModel(eventArgs.Member);
                databaseContext.Members.Add(guildMemberModel);
            }

            // Add Absent flag
            guildMemberModel.Flags |= MemberState.Absent;
            if (databaseContext.ChangeTracker.HasChanges())
            {
                await databaseContext.SaveChangesAsync();
            }
        }

        [DiscordEvent]
        public async Task OnGuildBanAddAsync(DiscordClient client, GuildBanAddEventArgs eventArgs)
        {
            DatabaseContext databaseContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            GuildMemberModel? guildMemberModel = databaseContext.Members.FirstOrDefault(member => member.UserId == eventArgs.Member.Id);
            if (guildMemberModel is null)
            {
                guildMemberModel = new GuildMemberModel(eventArgs.Member);
                databaseContext.Members.Add(guildMemberModel);
            }

            // Add Banned flag
            guildMemberModel.Flags |= MemberState.Banned;
            if (databaseContext.ChangeTracker.HasChanges())
            {
                await databaseContext.SaveChangesAsync();
            }
        }

        [DiscordEvent]
        public async Task OnGuildBanRemoveAsync(DiscordClient client, GuildBanRemoveEventArgs eventArgs)
        {
            DatabaseContext databaseContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            GuildMemberModel? guildMemberModel = databaseContext.Members.FirstOrDefault(member => member.UserId == eventArgs.Member.Id);
            if (guildMemberModel is null)
            {
                guildMemberModel = new GuildMemberModel(eventArgs.Member);
                databaseContext.Members.Add(guildMemberModel);
            }

            // Remove Banned flag
            guildMemberModel.Flags &= ~MemberState.Banned;
            if (databaseContext.ChangeTracker.HasChanges())
            {
                await databaseContext.SaveChangesAsync();
            }
        }

        [DiscordEvent]
        public async Task OnGuildMembersChunkedAsync(DiscordClient client, GuildMembersChunkEventArgs eventArgs)
        {
            DatabaseContext databaseContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            List<GuildMemberModel> guildMemberModels = await databaseContext.Members.Where(member => member.GuildId == eventArgs.Guild.Id).ToListAsync();
            foreach (DiscordMember member in eventArgs.Members)
            {
                GuildMemberModel? guildMemberModel = guildMemberModels.FirstOrDefault(memberModel => memberModel.UserId == member.Id);
                if (guildMemberModel is null)
                {
                    guildMemberModel = new GuildMemberModel(member);
                    databaseContext.Members.Add(guildMemberModel);
                }
                else
                {
                    // Remove Absent and Banned flags
                    guildMemberModel.Flags &= ~(MemberState.Absent | MemberState.Banned);
                }
            }

            if (databaseContext.ChangeTracker.HasChanges())
            {
                await databaseContext.SaveChangesAsync();
            }
        }
    }
}
