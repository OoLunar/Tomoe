using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tomoe.Db;

namespace Tomoe.Commands.Public
{
    [Group("tag"), Description("Sends a pre-specified message."), RequireGuild]
    public class Tags : BaseCommandModule
    {
        public Database Database { private get; set; }

        [GroupCommand]
        public async Task Send(CommandContext context, Tag tag)
        {
            Database.Tags.Attach(tag);
            tag.Uses += 1;
            if (tag.IsAlias)
            {
                Tag realTag = await Database.Tags.AsNoTracking().FirstOrDefaultAsync(realTag => realTag.Name == tag.AliasTo && realTag.GuildId == context.Guild.Id);
                await Program.SendMessage(context, realTag.Content);
            }
            else
            {
                await Program.SendMessage(context, tag.Content);
            }
            await Database.SaveChangesAsync();
            Database.Entry(tag).State = EntityState.Detached;
        }

        [Command("create"), Description("Creates a message for later use.")]
        public async Task Create(CommandContext context, string tagName, string tagContent)
        {
            Tag tag =
                // Get the tag name
                await Database.Tags.FirstOrDefaultAsync(tag => tag.Name == tagName.ToLowerInvariant())
                // Get the tag alias if no name was found.
                ?? await Database.Tags.FirstOrDefaultAsync(tag => tag.IsAlias && tag.AliasTo == tagName.ToLowerInvariant());

            if (tag != null)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Tag `{tagName}` already exists!"));
                return;
            }

            tag = new();
            tag.Content = tagContent;
            tag.GuildId = context.Guild.Id;
            tag.Name = tagName.ToLowerInvariant().Trim().Replace("\n", ". ");
            tag.OwnerId = context.User.Id;
            tag.TagId = Database.Tags.Where(tag => tag.GuildId == context.Guild.Id).Count();
            Database.Tags.Add(tag);
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, $"Tag `{tagName}` created!");
        }

        [Command("remove"), Description("Deletes a tag from the guild.")]
        public async Task Remove(CommandContext context, Tag tag)
        {
            if (tag.IsAlias)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Cannot alias the `{tag.Name}` alias!"));
            }
            else if (context.Member.HasPermission(Permissions.ManageMessages) || await IsGuildAdmin(context, Database) || tag.OwnerId == context.User.Id)
            {
                List<Tag> tags = await Database.Tags.Where(databaseTag => databaseTag.TagId == tag.TagId).ToListAsync();
                tags.Add(tag);
                Database.Tags.RemoveRange(tags);
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, $"Tag `{tag.Name}` has been deleted.");
            }
            else
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: You do not have permission to delete tag `{tag.Name}`"));
            }
        }

        [Command("edit"), Description("Edits a tag.")]
        public async Task Edit(CommandContext context, Tag tag, string newContent)
        {
            if (tag.IsAlias)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Alias `{tag.Name}` is not a tag!"));
            }
            else if (context.Member.HasPermission(Permissions.ManageMessages) || await IsGuildAdmin(context, Database) || tag.OwnerId == context.User.Id)
            {
                tag.Content = newContent;
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, $"Tag `{tag.Name}` has been edited.");
            }
            else
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: You do not have permission to edit tag `{tag.Name}`"));
            }
        }

        [Command("alias"), Description("Points a tag to another tag.")]
        public async Task Alias(CommandContext context, Tag tag, string tagName)
        {
            Tag aliasTag = new();
            aliasTag.IsAlias = true;
            aliasTag.AliasTo = tag.Name;
            aliasTag.GuildId = context.Guild.Id;
            aliasTag.Name = tagName.ToLowerInvariant().Trim();
            aliasTag.OwnerId = context.User.Id;
            aliasTag.TagId = tag.TagId;
            Database.Tags.Add(aliasTag);
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, $"Alias `{tagName}` has been created.");
        }

        [Command("remove_alias"), Description("Removes an alias that points to another tag.")]
        public async Task RemoveAlias(CommandContext context, Tag tag)
        {
            if (!tag.IsAlias)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Tag `{tag.Name}` is not an alias!"));
            }
            else if (context.Member.HasPermission(Permissions.ManageMessages) || await IsGuildAdmin(context, Database) || tag.OwnerId == context.User.Id)
            {
                Database.Tags.Remove(tag);
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, $"Tag `{tag.Name}` has been deleted.");
            }
            else
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: You do not have permission to delete alias `{tag.Name}`"));
            }
        }

        [Command("purge_aliases"), Description("Removes all aliases for a tag.")]
        public async Task PurgeAliases(CommandContext context, Tag tag)
        {
            if (tag.IsAlias)
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: Alias `{tag.Name}` is not a tag!"));
            }
            else if (context.Member.HasPermission(Permissions.ManageMessages) || await IsGuildAdmin(context, Database) || tag.OwnerId == context.User.Id)
            {
                List<Tag> tags = await Database.Tags.Where(databaseTag => databaseTag.TagId == tag.TagId && databaseTag.IsAlias).ToListAsync();
                Database.Tags.RemoveRange(tags);
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, $"All aliases for tag `{tag.Name}` have been purged!");
            }
            else
            {
                await Program.SendMessage(context, Formatter.Bold($"[Error]: You do not have permission to purge all aliases for tag `{tag.Name}`"));
            }
        }

        [Command("is_alias"), Description("Tests whether a tag is an alias or not.")]
        public async Task IsAlias(CommandContext context, Tag tag) => await Program.SendMessage(context, $"`{tag.Name}` is {(tag.IsAlias ? "an alias" : "a tag")}.");

        [Command("info"), Description("Sends general information about the tag.")]
        public async Task Info(CommandContext context, Tag tag)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Info on tag");
            embedBuilder.Title += $" `{tag.Name}`";
            embedBuilder.AddField("Name", tag.Name, true);
            embedBuilder.AddField("Owner", $"<@{tag.OwnerId}> ({tag.OwnerId})", true);
            embedBuilder.AddField("Created At", tag.CreatedAt.ToString() + " UTC", true);
            embedBuilder.AddField("Use Count", tag.Uses.ToString(), true);
            embedBuilder.AddField("Type", tag.IsAlias ? "Alias" : "Tag", true);
            if (tag.IsAlias)
            {
                embedBuilder.AddField("Alias To", tag.AliasTo, true);
                await Program.SendMessage(context, null, embedBuilder.Build());
            }
            else
            {
                List<string> aliasList = await Database.Tags.Where(databaseTag => databaseTag.TagId == tag.TagId && databaseTag.IsAlias && databaseTag.GuildId == context.Guild.Id).Select(tag => tag.Name).ToListAsync();
                embedBuilder.AddField("Total Alias Count", aliasList.Count.ToString(), true);
                if (aliasList.Count == 0)
                {
                    embedBuilder.AddField("Aliases", "None");
                    await Program.SendMessage(context, null, embedBuilder);
                    return;
                }

                string aliases = '`' + aliasList.Aggregate((total, part) => total + "`, `" + part) + '`';
                if (aliases.Length <= 2000)
                {
                    embedBuilder.AddField("Aliases", aliases);
                    await Program.SendMessage(context, null, embedBuilder);
                }
                else
                {
                    InteractivityExtension interactivity = context.Client.GetInteractivity();
                    IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(aliases, default, embedBuilder);
                    await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
                }
            }
        }

        [Command("get_author"), Description("Gets the owner of a tag.")]
        public async Task GetAuthor(CommandContext context, Tag tag)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, "Owns tag");
            DiscordMember tagOwner = await tag.OwnerId.GetMember(context.Guild);
            embedBuilder.Title = $"{tagOwner.DisplayName} {embedBuilder.Title} `{tag.Name}`";
            embedBuilder.Thumbnail = new()
            {
                Url = tagOwner.AvatarUrl,
            };

            await Program.SendMessage(context, null, embedBuilder.Build());
        }

        [Command("user"), Description("Gets general tag information about a user.")]
        public async Task User(CommandContext context, DiscordMember discordUser)
        {
            DiscordEmbedBuilder embedBuilder = new();
            embedBuilder.Color = new DiscordColor("#7b84d1");
            embedBuilder.Title = "Tag info on ".Titleize() + discordUser.DisplayName;
            embedBuilder.AddField("Total Tags Created", Database.Tags.Where(databaseTag => databaseTag.OwnerId == discordUser.Id).Count().ToString(), true);
            embedBuilder.AddField("Total Tags Guild Created", Database.Tags.Where(databaseTag => databaseTag.OwnerId == discordUser.Id && databaseTag.GuildId == context.Guild.Id).Count().ToString(), true);
            List<string> tagTitlesEnumerable = Database.Tags.Where(databaseTag => databaseTag.OwnerId == discordUser.Id).Select(databaseTag => databaseTag.Name).ToList();
            string tagTitles = tagTitlesEnumerable.Count == 0 ? "None" : '`' + tagTitlesEnumerable.Aggregate((total, part) => total + "`, `" + part) + '`';
            embedBuilder.AddField("Guild Tags", tagTitles, true);
            await Program.SendMessage(context, null, embedBuilder.Build());
        }

        public static async Task<bool> IsGuildAdmin(CommandContext context, Database database)
        {
            // Query the admin roles (server side)
            List<ulong> list = await database.GuildConfigs.Where(guildConfig => guildConfig.Id == context.Guild.Id).Select(guildConfig => guildConfig.AdminRoles).FirstOrDefaultAsync();
            // See if the user has any (client side)
            return list.Intersect(context.Member.Roles.Select(role => role.Id)).Any();
        }
    }
}