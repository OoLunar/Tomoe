using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

namespace Tomoe.Commands.Public
{
	[Group("tag")]
	[Description("Gets a tag's content.")]
	[RequireGuild]
	public class Tags : BaseCommandModule
	{

		[GroupCommand]
		public async Task Get(CommandContext context, [RemainingText] string tagTitle)
		{
			if (tagTitle.Length > 32)
			{
				_ = Program.SendMessage(context, "**[Error: Tag title too long.]**", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
			}
			else
			{
				_ = Program.SendMessage(context, Program.Database.Tags.Retrieve(context.Guild.Id, tagTitle.Trim().ToLowerInvariant()) ?? $"**[Error: \"{tagTitle}\" doesn't exist!]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
		}

		[Command("create"), Description("Creates a tag."), RequireGuild, TagCheck(false, TagType.Any, TagState.Missing)]
		public async Task Create(CommandContext context, string tagTitle, [RemainingText] string content)
		{
			Program.Database.Tags.Create(context.Guild.Id, context.User.Id, tagTitle.Trim().ToLowerInvariant(), content.Filter(ExtensionMethods.FilteringAction.CodeBlocksIgnore));
			_ = Program.SendMessage(context, $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" has been created!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
		}

		[Command("edit"), Description("Edits a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Edit(CommandContext context, string tagTitle, [RemainingText] string content)
		{
			Program.Database.Tags.Edit(context.Guild.Id, tagTitle, ExtensionMethods.Filter(content, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace));
			_ = Program.SendMessage(context, $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" successfully edited.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
		}

		[Command("delete"), Description("Deletes a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Delete(CommandContext context, string tagTitle)
		{
			Program.Database.Tags.Delete(context.Guild.Id, tagTitle);
			_ = Program.SendMessage(context, $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" successfully deleted.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
		}

		[Command("alias"), Description("Creates an alias for a tag."), RequireGuild, Aliases("create_alias"), TagCheck(false, TagType.Any, TagState.Missing)]
		public async Task CreateAlias(CommandContext context, string newName, string oldName)
		{
			if (!Program.Database.Tags.Exist(context.Guild.Id, oldName.ToLowerInvariant()))
			{
				_ = Program.SendMessage(context, $"Tag \"{oldName.ToLowerInvariant()}\" does not exist!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
			else if (Program.Database.Tags.IsAlias(context.Guild.Id, oldName.ToLowerInvariant()).Value)
			{
				_ = Program.SendMessage(context, $"**[Denied: Creating aliases of aliases aren't allowed. Use `>>tag create_alias {Program.Database.Tags.RealName(context.Guild.Id, oldName.ToLowerInvariant())}` to create the alias.]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
			else
			{
				Program.Database.Tags.CreateAlias(context.Guild.Id, context.User.Id, newName, oldName);
				_ = Program.SendMessage(context, $"Tag \"{newName.ToLowerInvariant()}\" has been created!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
		}

		[Command("delete_alias"), Description("Deletes a tag alias."), RequireGuild, TagCheck(true, TagType.Alias, TagState.Exists)]
		public async Task DeleteAlias(CommandContext context, string tagTitle)
		{
			Program.Database.Tags.DeleteAlias(context.Guild.Id, tagTitle);
			_ = Program.SendMessage(context, $"Tag alias \"{tagTitle.Trim().ToLowerInvariant()}\" successfully deleted.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
		}

		[Command("delete_all_aliases"), Description("Deletes all aliases for a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task DeleteAllAliases(CommandContext context, string tagTitle)
		{
			Program.Database.Tags.DeleteAllAliases(context.Guild.Id, tagTitle);
			_ = Program.SendMessage(context, $"All aliases for \"{tagTitle.Trim().ToLowerInvariant()}\" have been removed!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
		}

		[Command("exist"), Description("Tests if a tag exists."), RequireGuild, TagCheck(false, TagType.Any, TagState.Irrelevant)]
		public async Task Exist(CommandContext context, string tagTitle)
		{
			if (!Program.Database.Tags.Exist(context.Guild.Id, tagTitle.Trim().ToLowerInvariant()))
			{
				_ = Program.SendMessage(context, $"\"{tagTitle.Trim().ToLowerInvariant()}\" doesn't exist!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
			else
			{
				_ = Program.SendMessage(context, Program.Database.Tags.Exist(context.Guild.Id, tagTitle.Trim().ToLowerInvariant()) ? $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" does exist!" : $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" does not exist!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
		}

		[Command("is_alias"), Description("Tests if a tag is an alias."), RequireGuild, TagCheck(false, TagType.Any, TagState.Exists)]
		public async Task IsAlias(CommandContext context, string tagTitle) => Program.SendMessage(context, $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" is {(Program.Database.Tags.IsAlias(context.Guild.Id, tagTitle.Trim().ToLowerInvariant()).Value ? null : "not")} an alias.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);

		[Command("get_author"), Description("Gets the author of a tag."), RequireGuild, Aliases("author"), TagCheck(false, TagType.Any, TagState.Exists)]
		public async Task GetAuthor(CommandContext context, string tagTitle)
		{
			ulong tagAuthor = Program.Database.Tags.GetAuthor(context.Guild.Id, tagTitle.Trim().ToLowerInvariant()).Value;
			DiscordUser authorDiscordUser = await context.Client.GetUserAsync(tagAuthor);
			DiscordMember authorGuildMember = await context.Guild.GetMemberAsync(tagAuthor);
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
			embedBuilder.Title = tagTitle.Trim().ToLowerInvariant();
			if (authorGuildMember != null)
			{
				embedBuilder.Author = new()
				{
					Name = authorGuildMember.Username,
					IconUrl = authorGuildMember.AvatarUrl,
					Url = authorGuildMember.AvatarUrl
				};
				embedBuilder.Description = $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" belongs to {authorGuildMember.Mention}.";
			}
			else if (authorDiscordUser != null)
			{
				embedBuilder.Author = new()
				{
					Name = authorDiscordUser.Username,
					IconUrl = authorDiscordUser.AvatarUrl,
					Url = authorDiscordUser.AvatarUrl
				};
				embedBuilder.Description = $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" belongs to {authorDiscordUser.Mention}, however they are not currently present in the guild. This means you can claim the tag. See `>>help tag claim` for more information.";
			}
			else
			{
				embedBuilder.Author = new()
				{
					Name = "Unknown",
					IconUrl = context.User.DefaultAvatarUrl,
					Url = context.User.DefaultAvatarUrl
				};
				embedBuilder.Description = $"Tag \"{tagTitle.Trim().ToLowerInvariant()}\" is owned by <@{tagAuthor}> ({tagAuthor}), however they are not currently present in the guild. This means you can claim the tag. See `>>help tag claim` for more information.";
			}
			_ = Program.SendMessage(context, embedBuilder.Build());
		}

		[Command("claim"), Description("Claims a tag."), RequireGuild, TagCheck(false, TagType.Tag, TagState.Exists)]
		public async Task Claim(CommandContext context, string tagTitle)
		{
			if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages)))
			{
				Program.Database.Tags.Claim(context.Guild.Id, tagTitle.Trim().ToLowerInvariant(), context.User.Id);
				_ = Program.SendMessage(context, $"{context.User.Mention} has forcefully claimed tag \"{tagTitle.Trim().ToLowerInvariant()}\" using their admin powers.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
			else
			{
				ulong tagAuthor = Program.Database.Tags.GetAuthor(context.Guild.Id, tagTitle.Trim().ToLowerInvariant()).Value;
				DiscordMember guildAuthor = await context.Guild.GetMemberAsync(tagAuthor);
				if (guildAuthor != null)
				{
					_ = Program.SendMessage(context, "**[Denied: Tag author is still in the guild!]**");
				}
				else
				{
					Program.Database.Tags.Claim(context.Guild.Id, tagTitle.Trim().ToLowerInvariant(), context.User.Id);
					_ = Program.SendMessage(context, $"Due to the old tag author <@{tagAuthor}> ({tagAuthor}), leaving, the tag \"{tagTitle.Trim().ToLowerInvariant()}\" has been transferred to {context.User.Mention}", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				}
			}
		}

		[Command("transfer"), Description("Transfers tag ownership to another person."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Transfer(CommandContext context, string tagTitle, DiscordUser newAuthor)
		{
			if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages)))
			{
				Program.Database.Tags.Claim(context.Guild.Id, tagTitle.Trim().ToLowerInvariant(), newAuthor.Id);
				_ = Program.SendMessage(context, $"{context.User.Mention} forcefully transferred tag \"{tagTitle.Trim().ToLowerInvariant()}\" to {newAuthor.Mention} using their admin powers.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
			else
			{
				DiscordMember newAuthorMember = await context.Guild.GetMemberAsync(newAuthor.Id);
				if (newAuthorMember == null)
				{
					_ = Program.SendMessage(context, $"**[Denied: {newAuthor} isn't in the guild.]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				}
				else
				{
					Program.Database.Tags.Claim(context.Guild.Id, tagTitle.Trim().ToLowerInvariant(), newAuthor.Id);
					_ = Program.SendMessage(context, $"Due to the old tag author, {context.User.Mention}, willing letting go of tag \"{tagTitle.Trim().ToLowerInvariant()}\", ownership has now been transferred to {newAuthor.Mention}", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				}
			}
		}

		[Command("get_aliases"), Description("Gets all aliases of a tag."), RequireGuild, TagCheck(false, TagType.Tag, TagState.Exists)]
		public async Task AllAliases(CommandContext context, string tagTitle)
		{
			string[] userTags = Program.Database.Tags.GetAliases(context.Guild.Id, tagTitle.Trim().ToLowerInvariant()) ?? Array.Empty<string>();
			if (userTags.Length == 0)
			{
				userTags = new string[] { "No aliases currently exist!" };
			}
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.User.Username,
				IconUrl = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl,
				Url = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All aliases for tag \"{tagTitle.Trim().ToLowerInvariant()}\"";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", userTags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
		}

		[Command("user"), Description("Lists all the tags a person owns on the server."), RequireGuild]
		public async Task UserTags(CommandContext context, DiscordUser user)
		{
			string[] userTags = Program.Database.Tags.GetUser(context.Guild.Id, user.Id) ?? Array.Empty<string>();
			if (userTags.Length == 0)
			{
				userTags = new string[] { "No tags currently exist!" };
			}
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.User.Username,
				IconUrl = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl,
				Url = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All tags by {user.Username} on \"{context.Guild.Name}\"";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", userTags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
		}

		[Command("user"), Description("Lists all the tags for the current user on the server."), RequireGuild]
		public async Task UserTags(CommandContext context) => await UserTags(context, context.User);

		[Command("all"), Description("Lists all the tags in this server."), RequireGuild]
		public async Task All(CommandContext context)
		{
			string[] allTags = Program.Database.Tags.GetGuild(context.Guild.Id);
			if (allTags[0] == null)
			{
				allTags = new string[] { "No tags currently exist!" };
			}
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All tags for \"{context.Guild.Name}\"";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", allTags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
		}

		[Command("realname"), Description("Gets the original tag using an alias."), RequireGuild, TagCheck(false, TagType.Alias, TagState.Exists)]
		public async Task RealName(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			if (!Program.Database.Tags.IsAlias(context.User.Id, tagTitle).Value)
			{
				_ = Program.SendMessage(context, $"\"{tagTitle}\" is the original tag!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
			else
			{
				_ = Program.SendMessage(context, $"The original tag for the alias \"{tagTitle}\" is `>>tag {Program.Database.Tags.RealName(context.Guild.Id, tagTitle)}`", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			}
		}
	}

	public enum TagType
	{
		Tag,
		Alias,
		Any
	}

	public enum TagState
	{
		Exists,
		Missing,
		Irrelevant
	}

	[AttributeUsage((AttributeTargets.Method | AttributeTargets.Class), AllowMultiple = false)]
	public sealed class TagCheck : CheckBaseAttribute
	{
		private readonly bool RequireOwner;
		private readonly TagType RequireTagType = TagType.Any;
		private readonly TagState MustExist = TagState.Exists;

		public TagCheck(bool requireOwner, TagType requireTagType, TagState mustExist)
		{
			RequireOwner = requireOwner;
			RequireTagType = requireTagType;
			MustExist = mustExist;
		}

		public override async Task<bool> ExecuteCheckAsync(CommandContext context, bool isHelpCommand)
		{
			if (isHelpCommand)
			{
				return true;
			}
			string tagTitle = context.RawArgumentString.Split(' ')[0].Trim().ToLowerInvariant();
			if (tagTitle.Length > 32)
			{
				_ = Program.SendMessage(context, "**[Error: Tag title too long.]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				return false;
			}
			else if (MustExist == TagState.Exists && !Program.Database.Tags.Exist(context.Guild.Id, tagTitle))
			{
				_ = Program.SendMessage(context, $"**[Error: \"{tagTitle}\" doesn't exist!]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				return false;
			}
			else if (MustExist == TagState.Missing && Program.Database.Tags.Exist(context.Guild.Id, tagTitle))
			{
				_ = Program.SendMessage(context, $"**[Error: \"{tagTitle}\" already exists!]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				return false;
			}
			else if (RequireOwner && Program.Database.Tags.GetAuthor(context.Guild.Id, tagTitle) != context.User.Id || context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages)))
			{
				_ = Program.SendMessage(context, "**[Denied: You aren't the tag owner!]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				return false;
			}
			else if (RequireTagType == TagType.Tag && Program.Database.Tags.IsAlias(context.Guild.Id, tagTitle).Value)
			{
				_ = Program.SendMessage(context, "**[Denied: Tag isn't a tag.]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				return false;
			}
			else if (RequireTagType == TagType.Alias && !Program.Database.Tags.IsAlias(context.Guild.Id, tagTitle).Value)
			{
				_ = Program.SendMessage(context, "**[Denied: Tag isn't an alias.]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
