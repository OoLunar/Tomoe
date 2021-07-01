namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommandGroup("config", "Manages the bot settings.")]
        public partial class Config : SlashCommandModule
        {
            public Database Database { private get; set; }

            [SlashCommand("show", "Shows the current bot settings for the server.")]
            public async Task Show(InteractionContext context)
            {
                GuildConfig guildConfig = Database.GuildConfigs.First(databaseGuildConfig => databaseGuildConfig.Id == context.Guild.Id);

                string adminRoles = string.Join(", ", guildConfig.AdminRoles.Select(roleId => context.Guild.Roles[roleId].Mention));
                string allowedInvites = string.Join(", ", guildConfig.AllowedInvites.Select(invite => "https://discord.gg/" + invite));
                string ignoredChannels = string.Join(", ", guildConfig.IgnoredChannels.Select(channelId => context.Guild.Channels[channelId].Mention));

                DiscordEmbedBuilder embedBuilder = new()
                {
                    Title = context.Guild.Name + " Guild Config",
                    Color = new DiscordColor("#7b84d1")
                };
                embedBuilder.WithThumbnail(context.Guild.IconUrl?.Replace(".jpg", ".png?size=1024"));
                embedBuilder.AddField("Admin Roles", string.IsNullOrEmpty(adminRoles) ? "None." : adminRoles);
                embedBuilder.AddField("Allowed Invites", string.IsNullOrEmpty(allowedInvites) ? "None." : allowedInvites);
                embedBuilder.AddField("Ignored Channels", string.IsNullOrEmpty(ignoredChannels) ? "None." : ignoredChannels);
                embedBuilder.AddField("Antimeme Role", guildConfig.AntimemeRole == 0 ? "Not Set." : context.Guild.Roles[guildConfig.AntimemeRole].Mention, true);
                embedBuilder.AddField("Mute Role", guildConfig.MuteRole == 0 ? "Not Set." : context.Guild.Roles[guildConfig.MuteRole].Mention, true);
                embedBuilder.AddField("Voiceban Role", guildConfig.VoicebanRole == 0 ? "Not Set." : context.Guild.Roles[guildConfig.VoicebanRole].Mention, true);
                embedBuilder.AddField("AntiInvite", guildConfig.AntiInvite ? "Yes." : "No.", true);
                embedBuilder.AddField("AutoDehoist", guildConfig.AutoDehoist ? "Yes." : "No.", true);
                embedBuilder.AddField("AutoStrike", guildConfig.AutoStrike ? "Yes." : "No.", true);
                embedBuilder.AddField("Delete Bad Messages", guildConfig.DeleteBadMessages ? "Yes." : "No.", true);
                embedBuilder.AddField("Max Lines Per Message", guildConfig.MaxLinesPerMessage.ToMetric(), true);
                embedBuilder.AddField("Max Unique Mentions Per Message", guildConfig.MaxUniqueMentionsPerMessage.ToMetric(), true);
                embedBuilder.AddField("Persistent Roles", guildConfig.PersistentRoles ? "Yes." : "No.", true);
                embedBuilder.AddField("Progressive Strikes", guildConfig.ProgressiveStrikes ? "Yes." : "No.", true);

                DiscordWebhookBuilder responseBuilder = new();
                responseBuilder.AddEmbed(embedBuilder);
                await context.EditResponseAsync(responseBuilder);
            }
        }
    }
}