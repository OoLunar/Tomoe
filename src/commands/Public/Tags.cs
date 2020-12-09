using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

namespace Tomoe.Commands.Public {
    [Group("tag")]
    public class Tags : BaseCommandModule {
        private static Utils.Logger _logger = new Utils.Logger("Commands/Public/Tags");

        [GroupCommand]
        [Aliases("get")]
        [Description("Gets a tag's content.")]
        [RequireGuild]
        public async Task Get(CommandContext context, [RemainingText] string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else Program.SendMessage(context, Program.Database.Driver.Tags.Get(context.Guild.Id, tagTitle.ToLowerInvariant()) ?? $"\"{tagTitle}\" doesn't exist!", (ExtensionMethods.FilteringAction.CodeBlocksIgnore | ExtensionMethods.FilteringAction.AllMentions));
        }

        [Command("create")]
        [Description("Creates a tag.")]
        [RequireGuild]
        public async Task Create(CommandContext context, string tagTitle, [RemainingText] string content) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) {
                Program.Database.Driver.Tags.Create(context.Guild.Id, context.User.Id, tagTitle.ToLowerInvariant(), ExtensionMethods.Filter(content, context, (ExtensionMethods.FilteringAction.CodeBlocksIgnore | ExtensionMethods.FilteringAction.AllMentions)));
                Program.SendMessage(context, $"Tag \"{tagTitle.ToLowerInvariant()}\" has been created!");
            } else Program.SendMessage(context, $"Tag \"{tagTitle.ToLowerInvariant()}\" already exists!");
        }

        [Command("edit")]
        [Description("Edits a tag.")]
        [RequireGuild]
        public async Task Edit(CommandContext context, string tagTitle, [RemainingText] string content) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else if (Program.Database.Driver.Tags.IsAlias(context.Guild.Id, tagTitle.ToLowerInvariant()).Value) Program.SendMessage(context, $"**[Denied: Editing aliases aren't allowed. Try `>>tag edit {Program.Database.Driver.Tags.RealName(context.Guild.Id, tagTitle.ToLowerInvariant())}` instead.]**", (ExtensionMethods.FilteringAction.CodeBlocksIgnore | ExtensionMethods.FilteringAction.AllMentions));
            else if (Program.Database.Driver.Tags.GetAuthor(context.Guild.Id, tagTitle.ToLowerInvariant()) == context.User.Id || context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))) {
                Program.Database.Driver.Tags.Edit(context.Guild.Id, tagTitle, ExtensionMethods.Filter(content, context, (ExtensionMethods.FilteringAction.CodeBlocksIgnore | ExtensionMethods.FilteringAction.AllMentions)));
                Program.SendMessage(context, $"Tag \"{tagTitle.ToLowerInvariant()}\" successfully edited.");
            } else Program.SendMessage(context, "**[Denied: You aren't the tag owner!]**");
        }

        [Command("delete")]
        [Description("Deletes a tag.")]
        [RequireGuild]
        public async Task Delete(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else if (Program.Database.Driver.Tags.IsAlias(context.Guild.Id, tagTitle.ToLowerInvariant()).Value) Program.SendMessage(context, $"**[Denied: Use `>>tag delete_alias {Program.Database.Driver.Tags.RealName(context.Guild.Id, tagTitle.ToLowerInvariant())}` to delete aliases.]**", (ExtensionMethods.FilteringAction.CodeBlocksIgnore | ExtensionMethods.FilteringAction.AllMentions));
            else if (Program.Database.Driver.Tags.GetAuthor(context.Guild.Id, tagTitle.ToLowerInvariant()) == context.User.Id || context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))) {
                bool? isAlias = Program.Database.Driver.Tags.IsAlias(context.Guild.Id, tagTitle.ToLowerInvariant());
                if (isAlias.HasValue && isAlias.Value == true) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" is an alias. Use `>>tag delete_alias {tagTitle.ToLowerInvariant()}` instead.");
                else {
                    Program.Database.Driver.Tags.Delete(context.Guild.Id, tagTitle);
                    Program.SendMessage(context, $"Tag \"{tagTitle.ToLowerInvariant()}\" successfully deleted.");
                }
            } else Program.SendMessage(context, "**[Denied: You aren't the tag owner!]**");
        }

        [Command("alias")]
        [Description("Creates an alias for a tag.")]
        [RequireGuild]
        [Aliases("create_alias")]
        public async Task CreateAlias(CommandContext context, string newName, string oldName) {
            if (newName.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (Program.Database.Driver.Tags.Exist(context.Guild.Id, newName.ToLowerInvariant())) Program.SendMessage(context, $"Tag \"{newName.ToLowerInvariant()}\" already exists!");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, oldName.ToLowerInvariant())) Program.SendMessage(context, $"Tag \"{oldName.ToLowerInvariant()}\" does not exist!");
            else if (Program.Database.Driver.Tags.IsAlias(context.Guild.Id, oldName.ToLowerInvariant()).Value) Program.SendMessage(context, $"**[Denied: Creating aliases of aliases aren't allowed. Use `>>tag create_alias {Program.Database.Driver.Tags.RealName(context.Guild.Id, oldName.ToLowerInvariant())}` to create the alias.]**", (ExtensionMethods.FilteringAction.CodeBlocksIgnore | ExtensionMethods.FilteringAction.AllMentions));
            else {
                Program.Database.Driver.Tags.CreateAlias(context.Guild.Id, context.User.Id, newName, oldName);
                Program.SendMessage(context, $"Tag \"{newName.ToLowerInvariant()}\" has been created!");
            }
        }

        [Command("delete_alias")]
        [Description("Deletes a tag alias.")]
        [RequireGuild]
        public async Task DeleteAlias(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else if (Program.Database.Driver.Tags.GetAuthor(context.Guild.Id, tagTitle.ToLowerInvariant()) == context.User.Id || context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))) {
                bool? isAlias = Program.Database.Driver.Tags.IsAlias(context.Guild.Id, tagTitle.ToLowerInvariant());
                if (isAlias.HasValue && isAlias.Value == false) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" is not an alias, but the origin tag. Use `>>tag delete {tagTitle.ToLowerInvariant()}` instead.");
                else {
                    Program.Database.Driver.Tags.DeleteAlias(context.Guild.Id, tagTitle);
                    Program.SendMessage(context, $"Tag alias \"{tagTitle.ToLowerInvariant()}\" successfully deleted.");
                }
            } else Program.SendMessage(context, "**[Denied: You aren't the tag owner!]**");
        }

        [Command("delete_all_aliases")]
        [Description("Deletes all aliases for a tag.")]
        [RequireGuild]
        public async Task DeleteAllAliases(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))) {
                Program.Database.Driver.Tags.DeleteAllAliases(context.Guild.Id, tagTitle);
                Program.SendMessage(context, $"All aliases for \"{tagTitle.ToLowerInvariant()}\" have been removed!");
            } else Program.SendMessage(context, "**[Denied: You don't have the `Administrator` or `Manage Messages` permission!]**", (ExtensionMethods.FilteringAction.AllMentions | ExtensionMethods.FilteringAction.CodeBlocksIgnore));
        }

        [Command("exist")]
        [Description("Tests if a tag exists.")]
        [RequireGuild]
        public async Task Exist(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else Program.SendMessage(context, Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant()) ? $"Tag \"{tagTitle.ToLowerInvariant()}\" does exist!" : $"Tag \"{tagTitle.ToLowerInvariant()}\" does not exist!");
        }

        [Command("is_alias")]
        [Description("Tests if a tag is an alias")]
        [RequireGuild]
        public async Task IsAlias(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else {
                bool? isAlias = Program.Database.Driver.Tags.IsAlias(context.Guild.Id, tagTitle.ToLowerInvariant());
                if (!isAlias.HasValue) Program.SendMessage(context, $"Tag \"{tagTitle.ToLowerInvariant()}\" does not exist!");
                else Program.SendMessage(context, $"Tag \"{tagTitle.ToLowerInvariant()}\" is {(isAlias.Value ? null : "not")} an alias.");
            }
        }

        [Command("get_author")]
        [Description("Gets the author of a tag.")]
        [RequireGuild]
        [Aliases("author")]
        public async Task GetAuthor(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else {
                ulong? tagAuthor = Program.Database.Driver.Tags.GetAuthor(context.Guild.Id, tagTitle.ToLowerInvariant());
                DiscordUser authorDiscordUser = await context.Client.GetUserAsync(tagAuthor.Value);
                DiscordMember authorGuildMember = await context.Guild.GetMemberAsync(tagAuthor.Value);
                if (tagAuthor == null) Program.SendMessage(context, $"Tag \"{tagTitle.ToLowerInvariant()}\" does not exist!");
                else {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
                    if (authorGuildMember != null) {
                        embedBuilder.WithTitle(tagTitle.ToLowerInvariant());
                        embedBuilder.WithAuthor(authorGuildMember.Username, authorGuildMember.AvatarUrl, authorGuildMember.AvatarUrl);
                        embedBuilder.WithDescription($"Tag \"{tagTitle.ToLowerInvariant()}\" belongs to {authorGuildMember.Mention}.");
                    } else if (authorDiscordUser != null) {
                        embedBuilder.WithTitle(tagTitle.ToLowerInvariant());
                        embedBuilder.WithAuthor(authorDiscordUser.Username, authorDiscordUser.AvatarUrl, authorDiscordUser.AvatarUrl);
                        embedBuilder.WithDescription($"Tag \"{tagTitle.ToLowerInvariant()}\" belongs to {authorDiscordUser.Mention}, however they are not currently present in the guild. This means you can claim the tag. See `>>help tag claim` for more information.");
                    } else {
                        embedBuilder.WithTitle(tagTitle.ToLowerInvariant());
                        embedBuilder.WithAuthor("Unknown", context.User.DefaultAvatarUrl, context.User.DefaultAvatarUrl);
                        embedBuilder.WithDescription($"Tag \"{tagTitle.ToLowerInvariant()}\" is owned by <@{tagAuthor.Value}> ({tagAuthor.Value}), however they are not currently present in the guild. This means you can claim the tag. See `>>help tag claim` for more information.");
                    }
                    Program.SendMessage(context, embedBuilder.Build());
                }
            }
        }

        [Command("claim")]
        [Description("Claims a tag.")]
        [RequireGuild]
        public async Task Claim(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))) {
                Program.Database.Driver.Tags.Claim(context.Guild.Id, tagTitle.ToLowerInvariant(), context.User.Id);
                Program.SendMessage(context, $"{context.User.Mention} has forcefully claimed tag \"{tagTitle.ToLowerInvariant()}\" using their admin powers.");
            } else {
                ulong tagAuthor = Program.Database.Driver.Tags.GetAuthor(context.Guild.Id, tagTitle.ToLowerInvariant()).Value;
                DiscordMember guildAuthor = await context.Guild.GetMemberAsync(tagAuthor);
                if (guildAuthor != null) Program.SendMessage(context, "**[Denied: Tag author is still in the guild!]**");
                else {
                    Program.Database.Driver.Tags.Claim(context.Guild.Id, tagTitle.ToLowerInvariant(), context.User.Id);
                    Program.SendMessage(context, $"Due to the old tag author <@{tagAuthor}> ({tagAuthor}), leaving, the tag \"{tagTitle.ToLowerInvariant()}\" has been transferred to {context.User.Mention}", ExtensionMethods.FilteringAction.RoleMentions);
                }
            }
        }

        [Command("transfer")]
        [Description("Transfers tag ownership to another person.")]
        [RequireGuild]
        public async Task Transfer(CommandContext context, string tagTitle, DiscordUser newAuthor) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else if (Program.Database.Driver.Tags.GetAuthor(context.Guild.Id, tagTitle.ToLowerInvariant()) != context.User.Id) Program.SendMessage(context, "**[Denied: You aren't the tag owner!]**");
            else if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))) {
                Program.Database.Driver.Tags.Claim(context.Guild.Id, tagTitle.ToLowerInvariant(), newAuthor.Id);
                Program.SendMessage(context, $"{context.User.Mention} forcefully transferred tag \"{tagTitle.ToLowerInvariant()}\" to {newAuthor.Mention} using their admin powers.");
            } else {
                DiscordMember newAuthorMember = await context.Guild.GetMemberAsync(newAuthor.Id);
                if (newAuthorMember == null) Program.SendMessage(context, $"**[Denied: {newAuthor} isn't in the guild.]**", ExtensionMethods.FilteringAction.RoleMentions);
                else {
                    Program.Database.Driver.Tags.Claim(context.Guild.Id, tagTitle.ToLowerInvariant(), newAuthor.Id);
                    Program.SendMessage(context, $"Due to the old tag author, {context.User.Mention}, willing letting go of tag \"{tagTitle.ToLowerInvariant()}\", ownership has now been transferred to {newAuthor.Mention}", ExtensionMethods.FilteringAction.RoleMentions);
                }
            }
        }

        [Command("get_aliases")]
        [Description("Gets all aliases of a tag.")]
        [RequireGuild]
        public async Task AllAliases(CommandContext context, string tagTitle) {
            string[] userTags = Program.Database.Driver.Tags.GetAliases(context.Guild.Id, tagTitle.ToLowerInvariant()) ?? new string[] { };
            if (userTags.Length == 0) userTags = new string[] { "No aliases currently exist!" };
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithAuthor(context.User.Username, context.User.AvatarUrl ?? context.User.DefaultAvatarUrl, context.User.AvatarUrl ?? context.User.DefaultAvatarUrl);
            embedBuilder.WithTitle($"All aliases for tag \"{tagTitle.ToLowerInvariant()}\"");
            var interactivity = context.Client.GetInteractivity();
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", userTags), SplitType.Character, embedBuilder), timeoutoverride : TimeSpan.FromMinutes(2));
            return;
        }

        [Command("user")]
        [Description("Lists all the tags a person owns on the server.")]
        [RequireGuild]
        public async Task UserTags(CommandContext context, DiscordUser user) {
            string[] userTags = Program.Database.Driver.Tags.GetUser(context.Guild.Id, user.Id) ?? new string[] { };
            if (userTags.Length == 0) userTags = new string[] { "No tags currently exist!" };
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithAuthor(context.User.Username, context.User.AvatarUrl ?? context.User.DefaultAvatarUrl, context.User.AvatarUrl ?? context.User.DefaultAvatarUrl);
            embedBuilder.WithTitle($"All tags by {user.Username} on \"{context.Guild.Name}\"");
            var interactivity = context.Client.GetInteractivity();
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", userTags), SplitType.Character, embedBuilder), timeoutoverride : TimeSpan.FromMinutes(2));
            return;
        }

        [Command("user")]
        [Description("Lists all the tags for the current user on the server.")]
        [RequireGuild]
        public async Task UserTags(CommandContext context) => await UserTags(context, context.User);

        [Command("all")]
        [Description("Lists all the tags in this server.")]
        [RequireGuild]
        public async Task All(CommandContext context) {
            string[] allTags = Program.Database.Driver.Tags.GetGuild(context.Guild.Id);
            if (allTags[0] == null) allTags = new string[] { "No tags currently exist!" };
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.WithAuthor(context.Guild.Name, context.Guild.IconUrl ?? context.User.DefaultAvatarUrl, context.Guild.IconUrl ?? context.User.DefaultAvatarUrl);
            embedBuilder.WithTitle($"All tags for \"{context.Guild.Name}\"");
            var interactivity = context.Client.GetInteractivity();
            await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", allTags), SplitType.Character, embedBuilder), timeoutoverride : TimeSpan.FromMinutes(2));
            return;
        }

        [Command("realname")]
        [Description("Gets the original tag using an alias.")]
        [RequireGuild]
        public async Task RealName(CommandContext context, string tagTitle) {
            if (tagTitle.Length > 32) Program.SendMessage(context, "Tag title too long.");
            else if (!Program.Database.Driver.Tags.Exist(context.Guild.Id, tagTitle.ToLowerInvariant())) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" doesn't exist!");
            else if (!Program.Database.Driver.Tags.IsAlias(context.User.Id, tagTitle.ToLowerInvariant()).Value) Program.SendMessage(context, $"\"{tagTitle.ToLowerInvariant()}\" is the original tag!");
            else Program.SendMessage(context, $"The original tag for the alias \"{tagTitle.ToLowerInvariant()}\" is `>>tag {Program.Database.Driver.Tags.RealName(context.Guild.Id, tagTitle.ToLowerInvariant())}`", (ExtensionMethods.FilteringAction.AllMentions | ExtensionMethods.FilteringAction.CodeBlocksIgnore));
        }
    }
}