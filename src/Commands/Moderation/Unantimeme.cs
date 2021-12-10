namespace Tomoe.Commands
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : ApplicationCommandModule
    {
        [SlashCommand("unantimeme", "Removes an antimeme from the victim. They can now react, send embeds or upload files."), Hierarchy(Permissions.ManageMessages)]
        public async Task Unantimeme(InteractionContext context, [Option("victim", "Who to unantimeme?")] DiscordUser victim, [Option("reason", "Why is the victim being unantimemed?")] string reason = Constants.MissingReason)
        {
            GuildConfig guildConfig = Database.GuildConfigs.First(databaseGuildConfig => databaseGuildConfig.Id == context.Guild.Id);
            DiscordRole antimemeRole = null;
            bool databaseNeedsSaving = false; // Thank you! But our Database is in another castle!

            if (guildConfig.AntimemeRole == 0 || context.Guild.GetRole(guildConfig.AntimemeRole) == null)
            {
                await context.EditResponseAsync(new()
                {
                    Content = "Error: The antimeme role does not exist. Unable to remove what can't be found."
                });
                return;
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

            if (!databaseVictim.IsAntimemed)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {victim.Mention} is not antimemed!"
                });

                if (databaseNeedsSaving)
                {
                    await Database.SaveChangesAsync();
                }
                return;
            }

            databaseVictim.IsAntimemed = false;
            guildVictim ??= await victim.Id.GetMember(context.Guild);
            bool sentDm = await guildVictim.TryDmMember($"{context.User.Mention} ({context.User.Username}#{context.User.Discriminator}) has removed your antimeme in the guild {Formatter.Bold(context.Guild.Name)}.\nReason: {reason}\nNote: An antimeme prevents you from reacting to messages, sending embeds, uploading files, streaming in voice channels, and forces the push-to-talk restriction in voice channels.");

            if (guildVictim != null)
            {
                await guildVictim.RevokeRoleAsync(antimemeRole, $"{context.User.Mention} ({context.User.Username}#{context.User.Discriminator}) unantimemed {victim.Mention} ({victim.Username}#{victim.Discriminator}).\nReason: {reason}");
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
                Content = $"{victim.Mention} ({victim.Username}#{victim.Discriminator}) has been unantimemed{(sentDm ? "" : " (failed to dm)")}.\nReason: {reason}"
            });
        }
    }
}