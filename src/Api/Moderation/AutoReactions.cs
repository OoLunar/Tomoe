using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Api
{
    public partial class Moderation
    {
        public class AutoReactions
        {
            public static async Task<bool> Create(DiscordClient discordClient, ulong discordGuildId, ulong discordChannelId, ulong discordUserId, params string[] discordEmojiNames)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();

                List<string> databaseAutoReactions = database.AutoReactions.Where(databaseAutoReaction => databaseAutoReaction.GuildId == discordGuildId && databaseAutoReaction.ChannelId == discordChannelId && discordEmojiNames.Contains(databaseAutoReaction.EmojiName)).Select(autoReaction => autoReaction.EmojiName).ToList();
                discordEmojiNames = discordEmojiNames.Except(databaseAutoReactions).ToArray();
                bool autoReactionCreated = false;

                foreach (string discordEmojiName in discordEmojiNames)
                {
                    autoReactionCreated = true;
                    AutoReaction autoReaction = new();
                    autoReaction.EmojiName = discordEmojiName;
                    autoReaction.ChannelId = discordChannelId;
                    autoReaction.GuildId = discordGuildId;
                    database.AutoReactions.Add(autoReaction);
                }

                await ModLog(discordClient, discordGuildId, LogType.AutoReactionCreate, database, $"<@{discordUserId}> created new autoreactions in channel <#{discordChannelId}>: {string.Join(", ", discordEmojiNames.Select(emojiName => DiscordEmoji.FromName(discordClient, emojiName, true)))}");
                await database.SaveChangesAsync();
                return autoReactionCreated;
            }

            public static async Task<bool> Delete(DiscordClient discordClient, ulong discordGuildId, ulong discordChannelId, ulong discordUserId, params string[] discordEmojiNames)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();

                List<string> databaseAutoReactions = database.AutoReactions.Where(databaseAutoReaction => databaseAutoReaction.GuildId == discordGuildId && databaseAutoReaction.ChannelId == discordChannelId && discordEmojiNames.Contains(databaseAutoReaction.EmojiName)).Select(autoReaction => autoReaction.EmojiName).ToList();
                discordEmojiNames = discordEmojiNames.Except(databaseAutoReactions).ToArray();

                bool autoReactionRemoved = false;
                List<AutoReaction> removeDiscordAutoReactions = new();

                foreach (string emojiName in discordEmojiNames)
                {
                    autoReactionRemoved = true;
                    AutoReaction autoReaction = new();
                    autoReaction.EmojiName = emojiName;
                    autoReaction.ChannelId = discordChannelId;
                    autoReaction.GuildId = discordGuildId;
                    removeDiscordAutoReactions.Add(autoReaction);
                }

                if (autoReactionRemoved)
                {
                    database.AutoReactions.RemoveRange(removeDiscordAutoReactions);
                    await ModLog(discordClient, discordGuildId, LogType.AutoReactionCreate, database, $"<@{discordUserId}> removed the following autoreactions in channel <#{discordChannelId}>: {string.Join(", ", discordEmojiNames.Select(emojiName => DiscordEmoji.FromName(discordClient, emojiName, true)))}");
                    await database.SaveChangesAsync();
                }

                return autoReactionRemoved;
            }

            public static async Task<bool> Delete(DiscordClient discordClient, ulong discordGuildId, ulong discordChannelId, ulong discordUserId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                AutoReaction[] databaseAutoReactions = database.AutoReactions.Where(autoReaction => autoReaction.GuildId == discordGuildId && autoReaction.ChannelId == discordChannelId).ToArray();
                if (databaseAutoReactions.Length == 0)
                {
                    return false;
                }
                else
                {
                    database.AutoReactions.RemoveRange(databaseAutoReactions);
                    await ModLog(discordClient, discordGuildId, LogType.AutoReactionDelete, database, $"<@{discordUserId}> deleted all autoreactions in channel <#{discordChannelId}>!");
                    await database.SaveChangesAsync();
                    return true;
                }
            }

            public static List<AutoReaction> List(ulong discordGuildId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.AutoReactions.Where(autoReaction => autoReaction.GuildId == discordGuildId).ToList();
            }

            public static List<AutoReaction> List(ulong discordGuildId, ulong discordChannelId)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.AutoReactions.Where(autoReaction => autoReaction.GuildId == discordGuildId && autoReaction.ChannelId == discordChannelId).ToList();
            }

            public static List<AutoReaction> List(ulong discordGuildId, string discordEmojiName)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.AutoReactions.Where(autoReaction => autoReaction.GuildId == discordGuildId && autoReaction.EmojiName == discordEmojiName).ToList();
            }
        }
    }
}