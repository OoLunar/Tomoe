using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Interfaces;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class TempRole : ModerationCommand
    {
        public ExpirableService<TempRoleModel> ExpirableService { private get; set; } = null!;

        [Command("temprole")]
        [Description("Temporarily gives role to a user for the specified amount of time.")]
        [RequireGuild, RequirePermissions(Permissions.ManageRoles)]
        public async Task TempRoleAsync(CommandContext context, [Description("The user to give the role to.")] DiscordMember member, [Description("The role to give to the user.")] DiscordRole role, [Description("The amount of time to give the role to the user.")] DateTimeOffset expireDate)
        {
            if (!await CheckPermissionsAsync(context, Permissions.ManageRoles, member))
            {
                return;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            try
            {
                await member.GrantRoleAsync(role, $"Assigned by {context.Member!.Username}#{context.Member.Discriminator} until {expireDate.Humanize(now, CultureInfo.InvariantCulture)}.");
                Audit.Successful = true;
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to assign role {role.Id} to {member.Id}, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                await context.RespondAsync($"I was unable to give the role to the user, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                return;
            }

            TempRoleModel tempRole = new(context.Member.Id, context.Guild.Id, member.Id, role.Id, expireDate);
            await ExpirableService.AddAsync(tempRole);
            await context.RespondAsync($"Assigned {role.Name} to {member.Username}#{member.Discriminator} until {expireDate.Humanize(now, CultureInfo.InvariantCulture)}.");
        }
    }
}
