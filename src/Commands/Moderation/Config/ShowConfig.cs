using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    [Group("config"), RequireGuild, Description("Shows and sets settings for the guild."), RequireUserPermissions(Permissions.ManageGuild)]
    public partial class Config : BaseCommandModule
    {
        public Database Database { private get; set; }

        [GroupCommand]
        public async Task ShowConfig(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, "Config For");
            embedBuilder.Title += $" {context.Guild.Name}";
            embedBuilder.AddField("**Admin Roles**", string.Join(", ", guildConfig.AdminRoles.Select(role => $"<@&{role}>").DefaultIfEmpty("None set")), false);
            embedBuilder.AddField("**Allowed Invites**", string.Join(", ", guildConfig.AllowedInvites.Select(code => $"https://discord.gg/{code}").DefaultIfEmpty("None set")), false);
            embedBuilder.AddField("**Guild Prefixes**", string.Join(", ", guildConfig.Prefixes.Select(prefix => $"`{Formatter.Sanitize(prefix)}`").Append("`>>`")), false);
            embedBuilder.AddField("**Ignored Channels**", string.Join(", ", guildConfig.IgnoredChannels.Select(channel => $"<#{channel}>").DefaultIfEmpty("None set")), false);
            embedBuilder.AddField("**AntiInvite**", guildConfig.AntiInvite.ToString(), true);
            embedBuilder.AddField("**Auto Dehoist**", guildConfig.AutoDehoist.ToString(), true);
            embedBuilder.AddField("**Auto Strikes**", guildConfig.AutoStrike.ToString(), true);
            embedBuilder.AddField("**Delete Bad Messages**", guildConfig.AutoDelete.ToString(), true);
            embedBuilder.AddField("**Max Lines Per Message**", guildConfig.MaxLinesPerMessage.ToMetric(), true);
            embedBuilder.AddField("**Max Mentions Per Message**", guildConfig.MaxUniqueMentionsPerMessage.ToMetric(), true);
            //embedBuilder.AddField("**Progressive Strikes**", guildConfig.ProgressiveStrikes.ToString(), true);
            embedBuilder.AddField("**Show Permission Errors**", guildConfig.ShowPermissionErrors.ToString(), false);
            embedBuilder.AddField("**Antimeme Role**", guildConfig.AntimemeRole == 0 ? "Not set" : $"<@&{guildConfig.AntimemeRole}>", true);
            embedBuilder.AddField("**Mute Role**", guildConfig.MuteRole == 0 ? "Not set" : $"<@&{guildConfig.MuteRole}>", true);
            embedBuilder.AddField("**Voiceban Role**", guildConfig.VoicebanRole == 0 ? "Not set" : $"<@&{guildConfig.VoicebanRole}>", true);
            await Program.SendMessage(context, null, embedBuilder.Build());
        }
    }
}