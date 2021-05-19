namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class MessageRecieved
    {
        public static readonly Regex InviteRegex = new(@"disc(?:ord)?(?:(?:app)?\.com\/invite|(?:\.gg))\/([A-z0-9-]{2,})", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Handles max lines, max mentions and anti-invite.
        /// </summary>
        /// <param name="client">Used to grab guild settings and the user id.</param>
        /// <param name="eventArgs">Used to grab the guild, remove the message if required and potentionally strike the user.</param>
        /// <returns></returns>
        public static async Task Handler(DiscordClient client, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Author.Id == client.CurrentUser.Id
                || eventArgs.Guild == null
                || eventArgs.Message.WebhookMessage
                || eventArgs.Author.IsBot
                || (eventArgs.Author.IsSystem.HasValue && eventArgs.Author.IsSystem.Value)
            )
            {
                return;
            }

            DiscordMember authorMember = await eventArgs.Author.Id.GetMember(eventArgs.Guild);

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
            if (guildConfig == null
                || guildConfig.IgnoredChannels.Contains(eventArgs.Channel.Id)
                || authorMember.HasPermission(Permissions.ManageMessages)
                  || authorMember.HasPermission(Permissions.Administrator)
                  || eventArgs.Guild.OwnerId == eventArgs.Author.Id
                  || guildConfig.AdminRoles.ConvertAll(role => role.ToString()).Intersect(authorMember.Roles.ToList().ConvertAll(role => role.ToString())).Any()
              )
            {
                return;
            }

            int maxMentions = guildConfig.MaxUniqueMentionsPerMessage;
            int maxLines = guildConfig.MaxLinesPerMessage;

            if (maxMentions > -1 && (eventArgs.MentionedUsers.Count + eventArgs.MentionedRoles.Count) > maxMentions)
            {
                DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Please refrain from spamming pings.");

                if (guildConfig.AutoDelete)
                {
                    await eventArgs.Message.DeleteAsync("Exceeded max ping limit.");
                }

                if (guildConfig.AutoStrike)
                {
                    CommandsNextExtension commandsNext = client.GetCommandsNext();
                    Command command = commandsNext.FindCommand($"strike {eventArgs.Author.Mention} Please refrain from spamming pings.", out string args);
                    CommandContext context = commandsNext.CreateContext(message, ">>", command, args);
                    await commandsNext.ExecuteCommandAsync(context);
                }
            }

            if (maxLines > -1 && eventArgs.Message.Content.Split('\n').Length > maxLines)
            {
                DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Please refrain from spamming new lines.");

                if (guildConfig.AutoDelete)
                {
                    await eventArgs.Message.DeleteAsync("Exceeded max line limit.");
                }

                if (guildConfig.AutoStrike)
                {
                    CommandsNextExtension commandsNext = client.GetCommandsNext();
                    Command command = commandsNext.FindCommand($"strike {eventArgs.Author.Mention} Please refrain from spamming new lines.", out string args);
                    CommandContext context = commandsNext.CreateContext(message, ">>", command, args);
                    await commandsNext.ExecuteCommandAsync(context);
                }
            }

            if (guildConfig.AntiInvite)
            {
                Match messageInvites = InviteRegex.Match(eventArgs.Message.Content);
                if (messageInvites.Success)
                {
                    CaptureCollection invites = messageInvites.Captures;
                    foreach (Capture capture in invites)
                    {
                        if (!guildConfig.AllowedInvites.Contains(capture.Value))
                        {
                            await eventArgs.Message.DeleteAsync($"Invite {Formatter.InlineCode(capture.Value)} is not whitelisted.");
                            DiscordMessage message = await eventArgs.Message.RespondAsync($"{eventArgs.Author.Mention}: Please refrain from posting Discord invites.");

                            if (guildConfig.AutoDelete)
                            {
                                await eventArgs.Message.DeleteAsync("Posted Discord invite.");
                            }

                            if (guildConfig.AutoStrike)
                            {
                                CommandsNextExtension commandsNext = client.GetCommandsNext();
                                Command command = commandsNext.FindCommand($"strike {eventArgs.Author.Mention} Please refrain from posting Discord invites.", out string args);
                                CommandContext context = commandsNext.CreateContext(message, ">>", command, args);
                                await commandsNext.ExecuteCommandAsync(context);
                            }
                        }
                    }
                }
            }
        }
    }
}
