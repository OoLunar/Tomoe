using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tomoe.Models;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
    public class TempRole : BaseCommandModule
    {
        public DatabaseList<TempRoleModel, Guid> TempRoleModelList { private get; set; } = null!;
        public Logger<TempRole> Logger { private get; set; } = null!;

        [Command("temprole"), Description("Temporarily assign a role to a user for the specified amount of time."), RequireGuild]
        public async Task TempRoleAsync(CommandContext context, DiscordMember member, DiscordRole role, TimeSpan? timeSpan = null)
        {
            // Check if the executing user can give roles to the member.
            if (!context.Member.CanExecute(Permissions.ManageRoles, member))
            {
                await context.RespondAsync($"[Error]: You cannot assign roles to {member.Mention} due to Discord permissions!");
                return;
            }
            // Check if the role is higher than the assigners's highest role.
            else if (role.Position >= context.Member.Hierarchy)
            {
                await context.RespondAsync($"[Error]: {role.Mention} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))}) is higher than {context.Member.Mention}'s highest role!");
                return;
            }
            // Check if the member already has the role.
            else if (member.Roles.Contains(role))
            {
                await context.RespondAsync($"[Error]: {member.Mention} already has the role {role.Mention} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))})!");
                return;
            }
            // Check if the bot can give roles to the member.
            else if (!context.Guild.CurrentMember.CanExecute(Permissions.ManageRoles, member))
            {
                await context.RespondAsync($"[Error]: I cannot assign roles to {member.Mention} due to Discord permissions!");
                return;
            }
            // Check if the role is higher than the bot's highest role.
            else if (role.Position >= context.Guild.CurrentMember.Hierarchy)
            {
                await context.RespondAsync($"[Error]: {role.Mention} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))}) is higher than my highest role!");
                return;
            }

            timeSpan ??= TimeSpan.FromMinutes(5);
            try
            {
                await member.GrantRoleAsync(role, reason: $"Temporarily assigned by {context.Member.Mention} ({context.Member.Id}) for {timeSpan.Value.Humanize()}.");
            }
            catch (DiscordException error)
            {
                await context.RespondAsync($"[Error]: I was unable to assign the role {role.Mention} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))}) to {member.Mention}! Error: {error.WebResponse.ResponseCode}, {Formatter.InlineCode(error.JsonMessage)}");
                return;
            }

            TempRoleModel tempRoleModel = new()
            {
                GuildId = context.Guild.Id,
                RoleId = role.Id,
                Assignee = member.Id,
                Assigner = context.Member.Id,
                ExpiresAt = DateTime.UtcNow + timeSpan.Value
            };
            TempRoleModelList.Add(tempRoleModel);

            await context.RespondAsync($"{member.Mention} will now have the role {role.Mention} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))}) for {timeSpan.Value.Humanize()}.");
        }

        public static async Task RoleExpired(object? sender, TempRoleModel tempRoleModel)
        {
            if (!Program.BotReady)
            {
                return;
            }

            Logger<TempRole> logger = Program.ServiceProvider.GetService<Logger<TempRole>>()!;
            DiscordClient client = Program.DiscordShardedClient.GetShard(tempRoleModel.GuildId);
            DatabaseList<TempRoleModel, Guid> tempRoleModelList = (DatabaseList<TempRoleModel, Guid>)sender!;

            if (client == null)
            {
                logger.LogTrace("{TempRoleModelId}: Failed to get client for guild {GuildId}. Testing to see if the guild still exists...", tempRoleModel.Id, tempRoleModel.GuildId);
                try
                {
                    await Program.DiscordShardedClient.ShardClients[0].GetGuildAsync(tempRoleModel.GuildId);
                    client = Program.DiscordShardedClient.ShardClients[0];
                    logger.LogWarning("{TempRoleModelId}: Guild {GuildId} still exists. Must be a library bug.", tempRoleModel.Id, tempRoleModel.GuildId);
                }
                catch (NotFoundException) // Guild's been deleted.
                {
                    logger.LogDebug("{TempRoleModelId}: Failed to get guild {GuildId} from cache as it's been deleted.", tempRoleModel.Id, tempRoleModel.GuildId);
                    DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
                    database.TempRoles.RemoveRange(database.TempRoles.Where(x => x.GuildId == tempRoleModel.GuildId));
                    await database.SaveChangesAsync();
                    tempRoleModelList.Remove(tempRoleModel);
                    return;
                }
                catch (DiscordException error)
                {
                    logger.LogInformation(error, "{TempRoleModelId}: Failed to get guild {GuildId} from rest request. Error: (HTTP {HTTPCode}) {JsonError}", tempRoleModel.Id, tempRoleModel.GuildId, error.WebResponse.ResponseCode, error.JsonMessage);
                    return;
                }
                return;
            }

            DiscordGuild guild = client.Guilds[tempRoleModel.GuildId];
            if (guild.IsUnavailable)
            {
                logger.LogDebug("{TempRoleModelId}: Guild {GuildId} is unavailable, not revoking temporary role. Adding a 5 minute delay.", tempRoleModel.Id, tempRoleModel.GuildId);
                tempRoleModel.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
                tempRoleModelList.Update(tempRoleModel);
                return;
            }

            if (!guild.Members.TryGetValue(tempRoleModel.Assignee, out DiscordMember? assignee) || assignee == null)
            {
                logger.LogWarning("Failed to get member {Assignee} from guild {GuildId} from cache, going to try making a rest request.", tempRoleModel.Assignee, tempRoleModel.GuildId);
                try
                {
                    assignee = await guild.GetMemberAsync(tempRoleModel.Assignee);
                }
                catch (NotFoundException) // Assignee left.
                {
                    logger.LogWarning("Failed to get member {Assignee} from guild {GuildId} from cache as the member has left.", tempRoleModel.Assignee, tempRoleModel.GuildId);
                    DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
                    database.TempRoles.RemoveRange(database.TempRoles.Where(x => x.Assignee == tempRoleModel.Assignee && x.GuildId == tempRoleModel.GuildId));
                    await database.SaveChangesAsync();
                    tempRoleModelList.Remove(tempRoleModel);
                    return;
                }
                catch (DiscordException error)
                {
                    logger.LogError(error, "Failed to get member {Assignee} from guild {GuildId} from rest request. Error: (HTTP {HTTPCode}) {JsonError}", tempRoleModel.Assignee, tempRoleModel.GuildId, error.WebResponse.ResponseCode, error.JsonMessage);
                    return;
                }
                return;
            }

            if (!guild.Roles.TryGetValue(tempRoleModel.RoleId, out DiscordRole? role) || role == null)
            {
                logger.LogInformation("Failed to get role {RoleId} from guild {GuildId} from cache. Assuming it's deleted.", tempRoleModel.RoleId, tempRoleModel.GuildId);
                DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
                database.TempRoles.RemoveRange(database.TempRoles.Where(x => x.RoleId == tempRoleModel.RoleId && x.GuildId == tempRoleModel.GuildId));
                await database.SaveChangesAsync();
                tempRoleModelList.Remove(tempRoleModel);
                return;
            }

            if (!assignee.Roles.Contains(assignee.Guild.GetRole(tempRoleModel.RoleId)))
            {
                tempRoleModelList.Remove(tempRoleModel);
                return;
            }

            string dmMessage;
            try
            {
                await assignee.RevokeRoleAsync(role);
                dmMessage = $"{assignee.Mention} ({Formatter.InlineCode(assignee.Id.ToString(CultureInfo.InvariantCulture))})'s temporary role {role.Name} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))}) in {guild.Name} has sucessfully expired.";
            }
            catch (DiscordException error)
            {
                dmMessage = $"{assignee.Mention} ({Formatter.InlineCode(assignee.Id.ToString(CultureInfo.InvariantCulture))})'s temporary role {role.Name} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))}) in {guild.Name} has failed to correctly expire. This means the role was not removed, and you'll need to do it by hand. Error: (HTTP {error.WebResponse.ResponseCode}) {Formatter.InlineCode(error.Message)}";
                logger.LogError(error, "Failed to revoke role {RoleId} from member {Assignee} in guild {GuildId}. Error: (HTTP {HTTPCode}) {JsonError}", tempRoleModel.RoleId, tempRoleModel.Assignee, tempRoleModel.GuildId, error.WebResponse.ResponseCode, error.JsonMessage);
                return;
            }

            if (!guild.Members.TryGetValue(tempRoleModel.Assigner, out DiscordMember? assigner) || assigner == null)
            {
                logger.LogWarning("Failed to get member {Assigner} from guild {GuildId} from cache, going to try making a rest request.", tempRoleModel.Assigner, tempRoleModel.GuildId);
                try
                {
                    assigner = await guild.GetMemberAsync(tempRoleModel.Assigner);
                }
                catch (NotFoundException) // Assigner left.
                {
                    logger.LogWarning("Failed to get member {Assignee} from guild {GuildId} from cache as the member has left.", tempRoleModel.Assigner, tempRoleModel.GuildId);
                    return;
                }
                catch (DiscordException error)
                {
                    logger.LogError(error, "Failed to get member {Assignee} from guild {GuildId} from rest request. Error: (HTTP {HTTPCode}) {JsonError}", tempRoleModel.Assigner, tempRoleModel.GuildId, error.WebResponse.ResponseCode, error.JsonMessage);
                    return;
                }
            }

            try
            {
                await assigner.SendMessageAsync(dmMessage);
            }
            catch (DiscordException)
            {
                // Ignore.
            }
            tempRoleModelList.Remove(tempRoleModel);
        }
    }
}