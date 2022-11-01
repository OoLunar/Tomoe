using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Commands.Attributes;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    public sealed class MuteCommand : ApplicationCommandModule
    {
        public Database Database { private get; set; } = null!;

        [SlashCommand("mute", "Prevents a user from having any sort of interaction in the guild."), Hierarchy(Permissions.ManageMessages)]
        public async Task MuteAsync(InteractionContext context, [Option("victim", "Who to mute?")] DiscordUser victim, [Option("reason", "Why is the victim being muted?")] string reason = Constants.MissingReason)
        {
            GuildConfig guildConfig = Database.GuildConfigs.First(databaseGuildConfig => databaseGuildConfig.Id == context.Guild.Id);
            DiscordRole muteRole = null;
            bool databaseNeedsSaving = false; // Thank you! But our Database is in another castle!

            if (guildConfig.MuteRole == 0 || context.Guild.GetRole(guildConfig.MuteRole) == null)
            {
                bool createRole = await context.ConfirmAsync("Error: The mute role does not exist. Should one be created now?");
                if (createRole)
                {
                    muteRole = await context.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.VeryDarkGray, false, false, "Used for the mute command and config.");
                    await ConfigCommand.FixRolePermissionsAsync(context.Guild, context.Member, muteRole, CustomEvent.Mute, Database);
                    guildConfig.MuteRole = muteRole.Id;
                    databaseNeedsSaving = true;
                }
                else
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = "Error: No mute role exists, and I did not recieve permission to create it."
                    });
                    return;
                }
            }
            else
            {
                muteRole = context.Guild.GetRole(guildConfig.MuteRole);
            }

            GuildMember databaseVictim = Database.GuildMembers.FirstOrDefault(guildUser => guildUser.UserId == victim.Id && guildUser.GuildId == context.Guild.Id);
            DiscordMember guildVictim = null;
            if (databaseVictim == null)
            {
                guildVictim = await victim.Id.GetMemberAsync(context.Guild);
                databaseVictim = new()
                {
                    UserId = victim.Id,
                    GuildId = context.Guild.Id,
                    JoinedAt = guildVictim.JoinedAt
                };

                if (guildVictim != null)
                {
                    databaseVictim.Roles = guildVictim.Roles.Except(new[] { context.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).ToList();
                }

                databaseNeedsSaving = true;
            }

            if (databaseVictim.IsMuted)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {victim.Mention} is already muted!"
                });

                if (databaseNeedsSaving)
                {
                    await Database.SaveChangesAsync();
                }
                return;
            }

            databaseVictim.IsMuted = true;
            await Database.SaveChangesAsync();
            guildVictim ??= await victim.Id.GetMemberAsync(context.Guild);
            bool sentDm = await guildVictim.TryDmMemberAsync($"{context.User.Mention} ({context.User.Username}#{context.User.Discriminator}) has muted you in the guild {Formatter.Bold(context.Guild.Name)}.\nReason: {reason}\nNote: A mute prevents you from having any sort of interaction with the guild. It makes the entire guild readonly. You can't react, upload files, speak in voice channels, etc. If you believe this is a mistake, reach out to staff in their preferred methods.");

            if (guildVictim != null)
            {
                await guildVictim.GrantRoleAsync(muteRole, $"{context.User.Mention} ({context.User.Username}#{context.User.Discriminator}) muted {victim.Mention} ({victim.Username}#{victim.Discriminator}).\nReason: {reason}");
            }

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", context.Guild.Name },
                { "guild_count", Program.TotalMemberCount[context.Guild.Id].ToMetric() },
                { "guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_username", guildVictim.Username },
                { "victim_tag", guildVictim.Discriminator },
                { "victim_mention", guildVictim.Mention },
                { "victim_id", guildVictim.Id.ToString(CultureInfo.InvariantCulture) },
                { "victim_displayname", guildVictim.DisplayName },
                { "moderator_username", context.Member.Username },
                { "moderator_tag", context.Member.Discriminator },
                { "moderator_mention", context.Member.Mention },
                { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                { "moderator_displayname", context.Member.DisplayName },
                { "punishment_reason", reason }
            };
            await ModLogCommand.ModLogAsync(context.Guild, keyValuePairs, CustomEvent.Antimeme);

            await context.EditResponseAsync(new()
            {
                Content = $"{victim.Mention} ({victim.Username}#{victim.Discriminator}) has been muted{(sentDm ? "" : " (failed to dm)")}.\nReason: {reason}"
            });
        }
    }
}
