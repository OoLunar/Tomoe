using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tomoe.Models;
using Tomoe.Utils;

namespace Tomoe.Commands.Common
{
    [Group("reminder"), Aliases("remind")]
    public class Reminders : BaseCommandModule
    {
        public DatabaseContext Database { private get; init; } = null!;
        public DatabaseList<ReminderModel, int> ReminderList { private get; init; } = null!;
        public Logger<ReminderModel> Logger { private get; init; } = null!;

        [GroupCommand, Aliases("set", "me", "add"), Description("Sets a reminder using relative time.")]
        public Task SetAsync(CommandContext context, TimeSpan expiresIn, string? content = null) => SetAsync(context, DateTime.UtcNow.Add(expiresIn), content);

        [GroupCommand, Aliases("set", "me", "add"), Description("Sets a reminder using dates.")]
        public Task SetAsync(CommandContext context, DateTime expiresAt, string? content = null)
        {
            expiresAt = expiresAt.ToUniversalTime();
            if (expiresAt < DateTime.UtcNow)
            {
                return context.RespondAsync("You can't set a reminder in the past!");
            }

            ReminderModel? reminder = new()
            {
                UserId = context.User.Id,
                MessageLink = context.Message.JumpLink,
                Content = content,
                SetAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                Reply = true
            };

            ReminderList.Add(reminder);
            return context.RespondAsync($"Reminder set for {Formatter.Timestamp(expiresAt, TimestampFormat.ShortDateTime)}");
        }

        [Command("list"), Aliases("ls"), Description("Lists all reminders.")]
        public Task ListAsync(CommandContext context)
        {
            IEnumerable<ReminderModel> reminders = Database.Reminders.Where(x => x.UserId == context.User.Id).AsEnumerable();
            if (!reminders.Any())
            {
                return context.RespondAsync("You don't have any reminders!");
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = new DiscordColor("#7b84d1"),
                Author = new()
                {
                    Name = context.Member?.DisplayName ?? context.User.Username,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Footer = new()
                {
                    Text = $"The numbers in the {Formatter.InlineCode("code blocks")} are the reminder IDs. Example: {Formatter.InlineCode(reminders.First().Id.ToString(CultureInfo.InvariantCulture))}"
                }
            };

            StringBuilder stringBuilder = new();
            foreach (ReminderModel reminder in reminders)
            {
                stringBuilder.Append($"- `{reminder.Id.ToString(CultureInfo.InvariantCulture)}` expires ");
                if (reminder.ExpiresAt >= DateTime.UtcNow.AddDays(7))
                {
                    stringBuilder.Append("on " + Formatter.Timestamp(reminder.ExpiresAt, TimestampFormat.LongDateTime));
                }
                else
                {
                    stringBuilder.Append(Formatter.Timestamp(reminder.ExpiresAt, TimestampFormat.RelativeTime));
                }

                stringBuilder.AppendLine(": " + Formatter.MaskedUrl(reminder.Content ?? "No content", reminder.MessageLink, "Link to the original message."));
            }

            if (stringBuilder.Length > 2000)
            {
                InteractivityExtension interactivity = context.Client.GetInteractivity();
                return interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(stringBuilder.ToString(), SplitType.Line, embedBuilder));
            }
            else
            {
                return context.RespondAsync(embedBuilder.WithDescription(stringBuilder.ToString()));
            }
        }

        [Command("remove"), Aliases("rm", "delete", "del", "clear"), Description("Removes a reminder.")]
        public async Task RemoveAsync(CommandContext context, int id)
        {
            ReminderModel? reminder = Database.Reminders.FirstOrDefault(x => x.Id == id && x.UserId == context.User.Id);
            if (reminder == null)
            {
                await context.RespondAsync($"Reminder with id `{id}` was not found!");
                return;
            }

            Database.Reminders.Remove(reminder);
            await Database.SaveChangesAsync();
            await context.RespondAsync($"Reminder `{id}` removed.");
        }

        public static Task ReminderExpiredAsync(object? sender, ReminderModel reminder)
        {
            if (!Program.BotReady)
            {
                return Task.CompletedTask;
            }

            Logger<Reminders> logger = Program.ServiceProvider.GetService<Logger<Reminders>>()!;
            DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
            DatabaseList<ReminderModel, int> reminderModelList = (DatabaseList<ReminderModel, int>)sender!;
            return reminder.GuildId == null
                ? SendDmReminderAsync(reminderModelList, reminder, database, logger, Program.DiscordShardedClient.GetShard(0))
                : SendGuildReminderAsync(reminderModelList, reminder, logger, Program.DiscordShardedClient.GetShard(reminder.GuildId.Value));
        }

        private static async Task SendDmReminderAsync(DatabaseList<ReminderModel, int> reminderModelList, ReminderModel reminder, DatabaseContext database, Logger<Reminders> logger, DiscordClient client)
        {
            bool foundDmChannel = client.PrivateChannels.TryGetValue(reminder.UserId, out DiscordDmChannel? dm) && dm != null;
            IAsyncEnumerable<DiscordDmChannel> dmChannels = AsyncEnumerable.Empty<DiscordDmChannel>();
            if (foundDmChannel)
            {
                dmChannels.Prepend(dm);
            }
            else
            {
                IEnumerable<ulong> sharedGuilds = database.GuildMembers.Where(x => x.UserId == reminder.UserId && x.IsInGuild).Select(x => x.GuildId);
                dmChannels = GetDiscordDmChannelsAsync(client, logger, reminder.UserId, sharedGuilds);
            }

            bool dmSent = false;
            await foreach (DiscordDmChannel dmChannel in dmChannels)
            {
                try
                {
                    await dmChannel.SendMessageAsync($"Reminder set on {Formatter.Timestamp(reminder.SetAt, TimestampFormat.ShortDateTime)} ({Formatter.Timestamp(reminder.SetAt, TimestampFormat.RelativeTime)}): {reminder.Content}\nLink: {reminder.MessageLink}");
                    reminderModelList.Remove(reminder);
                    dmSent = true;
                    break;
                }
                catch (DiscordException)
                {
                    logger.LogWarning("Unable to DM {UserId} their reminder {MessageLink} due to lack of connections.", reminder.UserId, reminder.MessageLink);
                }
            }

            if (!dmSent)
            {
                logger.LogWarning("Unable to DM {UserId} their reminder {MessageLink} due to lack of connections.", reminder.UserId, reminder.MessageLink);
            }
        }

        private static async IAsyncEnumerable<DiscordDmChannel> GetDiscordDmChannelsAsync(DiscordClient client, Logger<Reminders> logger, ulong userId, IEnumerable<ulong> guildIds)
        {
            foreach (ulong guildId in guildIds)
            {
                if (!client.Guilds.TryGetValue(guildId, out DiscordGuild? guild) || guild == null)
                {
                    continue;
                }

                if (!guild.Members.TryGetValue(userId, out DiscordMember? member) || member == null)
                {
                    try
                    {
                        member = await guild.GetMemberAsync(userId);
                        break;
                    }
                    catch (DiscordException error)
                    {
                        logger.LogError(error, "Failed to get member with id {UserId} from guild with id {GuildId}: (HTTP {HTTPCode}) {JsonMessage}", userId, guildId, error.WebResponse.ResponseCode, error.JsonMessage);
                        continue;
                    }
                }

                if (member != null)
                {

                    DiscordDmChannel dmChannel;
                    try
                    {
                        dmChannel = await member.CreateDmChannelAsync();
                    }
                    catch (DiscordException)
                    {
                        continue;
                    }
                    yield return dmChannel;
                }

            }

            yield break;
        }

        private static async Task SendGuildReminderAsync(DatabaseList<ReminderModel, int> reminderModelList, ReminderModel reminder, Logger<Reminders> logger, DiscordClient client)
        {
            if (!client.Guilds.TryGetValue(reminder.GuildId!.Value, out DiscordGuild? guild) || guild == null)
            {
                await SendDmReminderAsync(reminderModelList, reminder, Program.ServiceProvider.GetService<DatabaseContext>()!, logger, client);
            }
            else if (!guild.Channels.TryGetValue(reminder.ChannelId, out DiscordChannel? channel) || channel == null)
            {
                await SendDmReminderAsync(reminderModelList, reminder, Program.ServiceProvider.GetService<DatabaseContext>()!, logger, client);
            }
            else if (!channel.PermissionsFor(guild.CurrentMember).HasPermission(Permissions.SendMessages))
            {
                await SendDmReminderAsync(reminderModelList, reminder, Program.ServiceProvider.GetService<DatabaseContext>()!, logger, client);
            }
            else
            {
                try
                {
                    await channel.SendMessageAsync($"Reminder set on {Formatter.Timestamp(reminder.SetAt, TimestampFormat.ShortDateTime)} ({Formatter.Timestamp(reminder.SetAt, TimestampFormat.RelativeTime)}): {reminder.Content}\nLink: {reminder.MessageLink}");
                    reminderModelList.Remove(reminder);
                }
                catch (DiscordException)
                {
                    await SendDmReminderAsync(reminderModelList, reminder, Program.ServiceProvider.GetService<DatabaseContext>()!, logger, client);
                }
            }
        }
    }
}
