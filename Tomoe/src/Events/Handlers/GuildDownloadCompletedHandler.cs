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
        private readonly DatabaseContext _database;

        public GuildDownloadCompletedHandler(ILogger<GuildDownloadCompletedHandler> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();
        }

        [DiscordEvent]
        public async Task OnGuildDownloadCompletedAsync(DiscordClient client, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            foreach (DiscordGuild discordGuild in guildDownloadCompletedEventArgs.Guilds.Values)
            {
                GuildModel? guildModel = await _database.Guilds.FindAsync(discordGuild.Id);
                if (guildModel is null)
                {
                    _database.Guilds.Add(new GuildModel(discordGuild));
                    _database.Members.AddRange(discordGuild.Members.Values.Select(member => new GuildMemberModel(member)));
                }
                else
                {
                    List<GuildMemberModel> newMembers = new();
                    IEnumerable<GuildMemberModel> guildMemberModels = await _database.Members.Where(member => member.GuildId == discordGuild.Id).ToListAsync();
                    foreach (DiscordMember discordMember in discordGuild.Members.Values)
                    {
                        GuildMemberModel? guildMemberModel = guildMemberModels.FirstOrDefault(member => member.UserId == discordMember.Id);
                        if (guildMemberModel is null)
                        {
                            newMembers.Add(new GuildMemberModel(discordMember));
                        }
                    }

                    _database.Members.AddRange(newMembers);
                }
            }

            if (_database.ChangeTracker.HasChanges())
            {
                await _database.SaveChangesAsync();
            }

            _logger.LogInformation("Guild download completed, handling a total of {GuildCount:N0} guilds.", guildDownloadCompletedEventArgs.Guilds.Count);
        }
    }
}
