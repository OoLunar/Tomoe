using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildDownloadCompletedHandler
    {
        private readonly ILogger<GuildDownloadCompletedHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        public GuildDownloadCompletedHandler(ILogger<GuildDownloadCompletedHandler> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        [DiscordEvent]
        public async Task OnGuildDownloadCompletedAsync(DiscordClient client, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            DatabaseContext databaseContext = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
            foreach (DiscordGuild discordGuild in guildDownloadCompletedEventArgs.Guilds.Values)
            {
                GuildModel? guildModel = await databaseContext.Guilds.FindAsync(discordGuild.Id);
                if (guildModel is null)
                {
                    databaseContext.Guilds.Add(new GuildModel(discordGuild));
                    databaseContext.Members.AddRange(discordGuild.Members.Values.Select(member => new GuildMemberModel(member)));
                }
                else
                {
                    List<GuildMemberModel> newMembers = new();
                    IEnumerable<GuildMemberModel> guildMemberModels = await databaseContext.Members.Where(member => member.GuildId == discordGuild.Id).ToListAsync();
                    foreach (DiscordMember discordMember in discordGuild.Members.Values)
                    {
                        GuildMemberModel? guildMemberModel = guildMemberModels.FirstOrDefault(member => member.UserId == discordMember.Id);
                        if (guildMemberModel is null)
                        {
                            newMembers.Add(new GuildMemberModel(discordMember));
                        }
                        else if (!guildMemberModel.RoleIds.SequenceEqual(discordMember.Roles.Select(role => role.Id)))
                        {
                            guildMemberModel.RoleIds = discordMember.Roles.Select(role => role.Id).ToArray();
                        }
                    }

                    databaseContext.Members.AddRange(newMembers);
                }
            }

            if (databaseContext.ChangeTracker.HasChanges())
            {
                await databaseContext.SaveChangesAsync();
            }

            _logger.LogInformation("Guild download completed, handling a total of {GuildCount:N0} guilds and {MemberCount:N0} members.", guildDownloadCompletedEventArgs.Guilds.Count, databaseContext.Members.Count(member => !member.Flags.HasFlag(MemberState.Absent | MemberState.Banned)));
        }
    }
}
