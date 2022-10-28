using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;
using Tomoe.Utilities.Converters;

namespace Tomoe.Commands
{
    public partial class Moderation : ApplicationCommandModule
    {
        [SlashCommand("temprole", "Temporarily gives a role to a member."), SlashRequirePermissions(Permissions.ManageRoles), SlashRequireGuild]
        public async Task TempRoleAsync(InteractionContext context, [Option("expire_time_or_date", "When to remove the role.")] string expireTimeOrDate, [Option("discord_role", "Which role to assign and remove.")] DiscordRole discordRole, [Option("discord_user", "Which user to give and take the role away from.")] DiscordUser discordUser)
        {
            if (context.Member.Hierarchy <= discordRole.Position)
            {
                await context.CreateResponseAsync("You cannot give a role that is higher than your own.", true);
                return;
            }
            else if (context.Guild.CurrentMember.Hierarchy <= discordRole.Position)
            {
                await context.CreateResponseAsync("I cannot give a role that is higher than my highest role.", true);
                return;
            }

            await context.DeferAsync();
            TempRoleModel tempRole = Database.TemporaryRoles.FirstOrDefault(x => x.GuildId == context.Guild.Id && x.Assignee == discordUser.Id && x.RoleId == discordRole.Id);
            if (tempRole != null)
            {
                DiscordWebhookBuilder builder = new();
                builder.WithContent($"That user already has {discordRole.Mention} as a temporary role. It's set to expire at {Formatter.Timestamp(tempRole.ExpiresAt, TimestampFormat.ShortDateTime)}.");
                builder.AddMentions(Array.Empty<IMention>());
                await context.EditResponseAsync(builder);
                return;
            }

            if (JsonTimeSpanConverter.TryParse(expireTimeOrDate, out TimeSpan? expireTime))
            {
                await TempRoleAsync(context, expireTime.Value, discordRole, await context.Guild.GetMemberAsync(discordUser.Id));
            }
            else if (DateTime.TryParse(expireTimeOrDate, out DateTime expireDate))
            {
                await TempRoleAsync(context, expireDate, discordRole, await context.Guild.GetMemberAsync(discordUser.Id));
            }
            else
            {
                await context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Invalid time or date format."));
                return;
            }
        }

        public async Task TempRoleAsync(InteractionContext context, TimeSpan timeSpan, DiscordRole discordRole, DiscordMember discordMember)
        {
            if (timeSpan.TotalMinutes < 1)
            {
                await context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The requested time of removal must be at least 1 minute."));
                return;
            }

            if (!discordMember.Roles.Contains(discordRole))
            {
                await discordMember.GrantRoleAsync(discordRole, $"Temporary role, requested by {context.Member.Id.ToString(CultureInfo.InvariantCulture)}");
            }

            TempRoleModel tempRole = new()
            {
                GuildId = context.Guild.Id,
                RoleId = discordRole.Id,
                Assignee = discordMember.Id,
                Assigner = context.Member.Id,
                ExpiresAt = DateTime.UtcNow + timeSpan
            };
            Database.TemporaryRoles.Add(tempRole);
            await Database.SaveChangesAsync();

            DiscordWebhookBuilder responseBuilder = new();
            responseBuilder.WithContent($"{discordMember.Mention} has been given the role {discordRole.Mention}. Expires {Formatter.Timestamp(timeSpan, TimestampFormat.RelativeTime)}.");
            responseBuilder.AddMentions(Array.Empty<IMention>());
            await context.EditResponseAsync(responseBuilder);
        }

        public async Task TempRoleAsync(InteractionContext context, DateTime dateTime, DiscordRole discordRole, DiscordMember discordMember)
        {
            if (!discordMember.Roles.Contains(discordRole))
            {
                await discordMember.GrantRoleAsync(discordRole, $"Temporary role, requested by {context.Member.Id.ToString(CultureInfo.InvariantCulture)}");
            }

            TempRoleModel tempRole = new()
            {
                GuildId = context.Guild.Id,
                RoleId = discordRole.Id,
                Assignee = discordMember.Id,
                Assigner = context.Member.Id,
                ExpiresAt = dateTime.ToUniversalTime()
            };
            Database.TemporaryRoles.Add(tempRole);
            await Database.SaveChangesAsync();

            DiscordWebhookBuilder responseBuilder = new();
            responseBuilder.WithContent($"{discordMember.Mention} has been given the role {discordRole.Mention}. Expires on {Formatter.Timestamp(dateTime, TimestampFormat.ShortDateTime)}.");
            responseBuilder.AddMentions(Array.Empty<IMention>());
            await context.EditResponseAsync(responseBuilder);
        }

        public static async void TempRoleEventAsync(object sender, EventArgs eventArgs)
        {
            Database database = Program.ServiceProvider.GetService<Database>();

            IEnumerable<TempRoleModel> expiredRoles = database.TemporaryRoles.Where(x => x.ExpiresAt <= DateTime.UtcNow.AddMinutes(2));
            bool changeDatabase = false;
            foreach (TempRoleModel tempRole in expiredRoles)
            {
                if (tempRole.ExpiresAt >= DateTime.UtcNow)
                {
                    continue;
                }

                DiscordGuild guild = null;
                foreach (DiscordClient client in Program.Client.ShardClients.Values)
                {
                    if (client.Guilds.TryGetValue(tempRole.GuildId, out DiscordGuild foundGuild))
                    {
                        guild = foundGuild;
                        break;
                    }
                }

                if (guild == null)
                {
                    // We must've been removed from the guild, just remove the temporary role from the database.
                    database.TemporaryRoles.Remove(tempRole);
                    continue;
                }

                DiscordRole role = guild.GetRole(tempRole.RoleId);
                DiscordMember member = await guild.GetMemberAsync(tempRole.Assignee);
                if (member == null)
                {
                    // The member has left the guild, but they may return someday.
                    continue;
                }

                await member.RevokeRoleAsync(role, $"Temporary role expired.");
                try { await member.SendMessageAsync($"Your temporary role {role.Name} ({Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture))}), given to you by <@{tempRole.Assigner}> ({Formatter.InlineCode(tempRole.Assigner.ToString(CultureInfo.InvariantCulture))}), has expired."); }
                catch { }
                database.TemporaryRoles.Remove(tempRole);
                changeDatabase = true;
            }

            if (changeDatabase)
            {
                await database.SaveChangesAsync();
            }
        }
    }
}
