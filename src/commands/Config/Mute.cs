using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Commands.Listeners;
using Tomoe.Utils;

namespace Tomoe.Commands.Config {
    [Group("config")]
    public class Mute : BaseCommandModule {
        private static Logger _logger = new Logger("Config/Mute");

        [Command("mute"), Description("Sets up or assigns the mute role.")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [RequireGuild]
        public async Task SetupMute(CommandContext context, DiscordRole muteRole) {
            ulong? previousMuteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
            if (previousMuteRoleId.HasValue) {
                DiscordRole previousMuteRole = context.Guild.GetRole(previousMuteRoleId.Value);
                DiscordEmoji thumbsUp = DiscordEmoji.FromName(context.Client, "thumbsup");
                DiscordEmoji thumbsDown = DiscordEmoji.FromName(context.Client, "thumbsdown");
                ReactionAdded.Queue createNewRole = new ReactionAdded.Queue();
                createNewRole.MessageId = context.Message.Id;
                createNewRole.User = context.User;
                createNewRole.Emojis = new DiscordEmoji[] { thumbsUp, thumbsDown };
                createNewRole.Action = new ReactionAdded.ReactionHandler(emoji => {
                    if (emoji == thumbsUp) {
                        Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
                        _logger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
                        fixMuteRolePermissions(context, muteRole);
                        Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");
                    } else if (emoji == thumbsDown) {
                        _logger.Trace($"Set previous mute role {previousMuteRole.Name} ({previousMuteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
                        fixMuteRolePermissions(context, previousMuteRole);
                        Program.SendMessage(context, $"{previousMuteRole.Mention} is now set as the mute role.");
                    }
                });
                DiscordMessage discordMessage = Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it with {muteRole.Mention}?");
                await discordMessage.CreateReactionAsync(thumbsUp);
                await discordMessage.CreateReactionAsync(thumbsDown);
                ReactionAdded.QueueList.Add(createNewRole);
            } else {
                Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
                DiscordMessage discordMessage = Program.SendMessage(context, $"Setting role permissions for {muteRole.Mention}...");
                await fixMuteRolePermissions(context, muteRole);
                discordMessage.ModifyAsync($"{muteRole.Mention} is now set as the mute role.");
            }
        }

        [Command("mute")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [RequireBotPermissions(Permissions.ManageRoles)]
        [RequireGuild]
        public async Task SetupMute(CommandContext context) {
            DiscordMessage message = Program.SendMessage(context, "Creating mute role...") as DiscordMessage;
            DiscordRole muteRole = await context.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be muted.");
            _logger.Trace($"Created mute role '{muteRole.Name}' ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
            message.ModifyAsync($"{context.User.Mention}: Overriding channel permissions...");
            await fixMuteRolePermissions(context, muteRole);
            message.ModifyAsync($"{context.User.Mention}: Done! Mute role is now {muteRole.Mention}");
        }

        public static Task fixMuteRolePermissions(CommandContext context, DiscordRole muteRole) {
            foreach (DiscordChannel channel in context.Guild.Channels.Values) {
                switch (channel.Type) {
                    case ChannelType.Text:
                        _logger.Trace($"Overwriting permission {Permissions.SendMessages.ToString()} and {Permissions.AddReactions} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type.ToString()} channel {channel.Name} ({channel.Id}) for {context.Guild.Name} ({context.Guild.Id})...");
                        channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions, "Disallows users to send messages/communicate through reactions.").ConfigureAwait(false).GetAwaiter();
                        break;
                    case ChannelType.Voice:
                        _logger.Trace($"Overwriting permission {Permissions.Speak.ToString()} and {Permissions.Stream.ToString()} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type.ToString()} channel {channel.Name} ({channel.Id}) for {context.Guild.Name} ({context.Guild.Id})...").ConfigureAwait(false).GetAwaiter();
                        channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Speak | Permissions.Stream, "Disallows users to communicate in voice channels and through streams.");
                        break;
                    case ChannelType.Category:
                        _logger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak.ToString()} and {Permissions.Stream.ToString()} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type.ToString()} channel {channel.Name} ({channel.Id}) for {context.Guild.Name} ({context.Guild.Id})...").ConfigureAwait(false).GetAwaiter();
                        channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream, "Disallows users to send messages/communicate through reactions/voice channels and through streams.");
                        break;
                }
            }
            return Task.CompletedTask;
        }
    }
}