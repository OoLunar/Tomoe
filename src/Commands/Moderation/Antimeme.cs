namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : ApplicationCommandModule
    {
        [SlashCommand("antimeme", "Forces the user to use only text. No more reactions, embeds or uploading files."), Hierarchy(Permissions.ManageMessages)]
        public async Task Antimeme(InteractionContext context, [Option("victim", "Who to antimeme?")] DiscordUser victim, [Option("reason", "Why is the victim being antimemed?")] string reason = Constants.MissingReason)
        {
            GuildConfig guildConfig = Database.GuildConfigs.First(databaseGuildConfig => databaseGuildConfig.Id == context.Guild.Id);
            DiscordRole antimemeRole = null;
            bool databaseNeedsSaving = false; // Thank you! But our Database is in another castle!

            if (guildConfig.AntimemeRole == 0 || context.Guild.GetRole(guildConfig.AntimemeRole) == null)
            {
                bool createRole = await context.Confirm("Error: The antimeme role does not exist. Should one be created now?");
                if (createRole)
                {
                    antimemeRole = await context.Guild.CreateRoleAsync("Antimemed", Permissions.None, DiscordColor.VeryDarkGray, false, false, "Used for the antimeme command and config.");
                    await Config.FixRolePermissions(context.Guild, context.Member, antimemeRole, CustomEvent.Antimeme, Database);
                    guildConfig.AntimemeRole = antimemeRole.Id;
                    databaseNeedsSaving = true;
                }
                else
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = "Error: No antimeme role exists, and I did not recieve permission to create it."
                    });
                    return;
                }
            }
            else
            {
                antimemeRole = context.Guild.GetRole(guildConfig.AntimemeRole);
            }

            GuildMember databaseVictim = Database.GuildMembers.FirstOrDefault(guildUser => guildUser.UserId == victim.Id && guildUser.GuildId == context.Guild.Id);
            DiscordMember guildVictim = null;
            if (databaseVictim == null)
            {
                guildVictim = await victim.Id.GetMember(context.Guild);
                databaseVictim = new()
                {
                    UserId = victim.Id,
                    GuildId = context.Guild.Id
                };

                if (guildVictim != null)
                {
                    databaseVictim.Roles = guildVictim.Roles.Except(new[] { context.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).ToList();
                    databaseVictim.JoinedAt = guildVictim.JoinedAt.UtcDateTime;
                }

                databaseNeedsSaving = true;
            }

            if (databaseVictim.IsAntimemed)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {victim.Mention} is already antimemed!"
                });

                if (databaseNeedsSaving)
                {
                    await Database.SaveChangesAsync();
                }
                return;
            }

            databaseVictim.IsAntimemed = true;
            guildVictim ??= await victim.Id.GetMember(context.Guild);
            bool sentDm = await guildVictim.TryDmMember($"{context.User.Mention} ({context.User.Username}#{context.User.Discriminator}) has given you an antimeme in the guild {Formatter.Bold(context.Guild.Name)}.\nReason: {reason}\nNote: An antimeme prevents you from reacting to messages, sending embeds, uploading files, streaming in voice channels, and forces the push-to-talk restriction in voice channels.");

            if (guildVictim != null)
            {
                await guildVictim.GrantRoleAsync(antimemeRole, $"{context.User.Mention} ({context.User.Username}#{context.User.Discriminator}) antimemed {victim.Mention} ({victim.Username}#{victim.Discriminator}).\nReason: {reason}");
            }

            Dictionary<string, string> keyValuePairs = new();
            keyValuePairs.Add("guild_name", context.Guild.Name);
            keyValuePairs.Add("guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric());
            keyValuePairs.Add("guild_id", context.Guild.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("victim_username", guildVictim.Username);
            keyValuePairs.Add("victim_tag", guildVictim.Discriminator);
            keyValuePairs.Add("victim_mention", guildVictim.Mention);
            keyValuePairs.Add("victim_id", guildVictim.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("victim_displayname", guildVictim.DisplayName);
            keyValuePairs.Add("moderator_username", context.Member.Username);
            keyValuePairs.Add("moderator_tag", context.Member.Discriminator);
            keyValuePairs.Add("moderator_mention", context.Member.Mention);
            keyValuePairs.Add("moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture));
            keyValuePairs.Add("moderator_displayname", context.Member.DisplayName);
            keyValuePairs.Add("punishment_reason", reason);
            await ModLog(context.Guild, keyValuePairs, CustomEvent.Antimeme, Database);

            await context.EditResponseAsync(new()
            {
                Content = $"{victim.Mention} ({victim.Username}#{victim.Discriminator}) has been antimemed{(sentDm ? "" : " (failed to dm)")}.\nReason: {reason}"
            });
        }
    }
}