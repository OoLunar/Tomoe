using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	[Group("tag"), Description("Gets a tag's content."), RequireGuild]
	public class Tags : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Tags");

		[GroupCommand]
		public async Task Get(CommandContext context, [RemainingText] string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			if (tagTitle.Length > 32)
			{
				_logger.Trace("Tag title was too long!");
				_ = Program.SendMessage(context, "**[Error: Tag title too long.]**", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
				_logger.Trace("Message sent!");
			}
			else
			{
				_logger.Trace($"Retrieving tag \"{tagTitle}\"...");
				_ = Program.SendMessage(context, Program.Database.Tags.Retrieve(context.Guild.Id, tagTitle) ?? $"**[Error: \"{tagTitle}\" doesn't exist!]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
		}

		[Command("create"), Description("Creates a tag."), RequireGuild, TagCheck(false, TagType.Any, TagState.Missing)]
		public async Task Create(CommandContext context, string tagTitle, [RemainingText] string content)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Program.Database.Tags.Create(context.Guild.Id, context.User.Id, tagTitle, content.Filter(ExtensionMethods.FilteringAction.CodeBlocksIgnore));
			_ = Program.SendMessage(context, $"Tag \"{tagTitle}\" has been created!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
		}

		[Command("edit"), Description("Edits a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Edit(CommandContext context, string tagTitle, [RemainingText] string content)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Editing tag \"{tagTitle}\"...");
			Program.Database.Tags.Edit(context.Guild.Id, tagTitle, ExtensionMethods.Filter(content, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace));
			_ = Program.SendMessage(context, $"Tag \"{tagTitle}\" successfully edited.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			_logger.Trace("Message sent!");
		}

		[Command("delete"), Description("Deletes a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Delete(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Tag \"{tagTitle}\" is being deleted...");
			Program.Database.Tags.Delete(context.Guild.Id, tagTitle);
			_ = Program.SendMessage(context, $"Tag \"{tagTitle}\" successfully deleted.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			_logger.Trace("Message sent!");
		}

		[Command("alias"), Description("Creates an alias for a tag."), RequireGuild, TagCheck(false, TagType.Any, TagState.Missing)]
		public async Task Alias(CommandContext context, string newName, string oldName)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			newName = newName.Trim().ToLowerInvariant();
			oldName = oldName.Trim().ToLowerInvariant();

			_logger.Trace($"Testing if tag \"{oldName}\" exists...");
			if (!Program.Database.Tags.Exist(context.Guild.Id, oldName))
			{
				_logger.Trace($"Tag \"{oldName}\" does not exist...");
				_ = Program.SendMessage(context, $"Tag \"{oldName}\" does not exist!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
			else if (Program.Database.Tags.IsAlias(context.Guild.Id, oldName).Value)
			{
				_logger.Trace($"Tag \"{oldName}\" is an alias...");
				_ = Program.SendMessage(context, $"**[Denied: Creating aliases of aliases aren't allowed. Use `>>tag create_alias {Program.Database.Tags.RealName(context.Guild.Id, oldName)}` to create the alias.]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
			else
			{
				_logger.Trace($"Creating new alias \"{newName}\"...");
				Program.Database.Tags.CreateAlias(context.Guild.Id, context.User.Id, newName, oldName);
				_ = Program.SendMessage(context, $"Tag \"{newName.ToLowerInvariant()}\" has been created!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
		}

		[Command("delete_alias"), Description("Deletes a tag alias."), RequireGuild, TagCheck(true, TagType.Alias, TagState.Exists)]
		public async Task DeleteAlias(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Deleting alias \"{tagTitle}\"...");
			Program.Database.Tags.DeleteAlias(context.Guild.Id, tagTitle);
			_ = Program.SendMessage(context, $"Tag alias \"{tagTitle.Trim().ToLowerInvariant()}\" successfully deleted.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			_logger.Trace("Message sent!");
		}

		[Command("delete_all_aliases"), Description("Deletes all aliases for a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task DeleteAllAliases(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Deleting all aliases for tag \"{tagTitle}\"...");
			Program.Database.Tags.DeleteAllAliases(context.Guild.Id, tagTitle);
			_ = Program.SendMessage(context, $"All aliases for \"{tagTitle.Trim().ToLowerInvariant()}\" have been removed!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			_logger.Trace("Message sent!");
		}

		[Command("exist"), Description("Tests if a tag exists."), RequireGuild, TagCheck(false, TagType.Any, TagState.Irrelevant)]
		public async Task Exist(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Testing if tag \"{tagTitle}\" exists...");
			if (!Program.Database.Tags.Exist(context.Guild.Id, tagTitle))
			{
				_logger.Trace($"Tag \"{tagTitle}\" doesn't exist...");
				_ = Program.SendMessage(context, $"\"{tagTitle}\" doesn't exist!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
			else
			{
				_logger.Trace($"Tag \"{tagTitle}\" does exist!");
				_ = Program.SendMessage(context, $"Tag \"{tagTitle}\" does exist!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
		}

		[Command("is_alias"), Description("Tests if a tag is an alias."), RequireGuild, TagCheck(false, TagType.Any, TagState.Exists)]
		public async Task IsAlias(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Testing if tag \"{tagTitle}\" exists...");
			_ = Program.SendMessage(context, $"Tag \"{tagTitle}\" is {(Program.Database.Tags.IsAlias(context.Guild.Id, tagTitle).Value ? null : "not")} an alias.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
			_logger.Trace("Message sent!");
		}

		[Command("get_author"), Description("Gets the author of a tag."), RequireGuild, Aliases("getauthor", "author"), TagCheck(false, TagType.Any, TagState.Exists)]
		public async Task GetAuthor(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace("Creating embed...");
			ulong tagAuthor = Program.Database.Tags.GetAuthor(context.Guild.Id, tagTitle).Value;
			DiscordUser authorDiscordUser = await context.Client.GetUserAsync(tagAuthor);
			DiscordMember authorGuildMember = await context.Guild.GetMemberAsync(tagAuthor);
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Title = tagTitle;
			if (authorGuildMember != null)
			{
				embedBuilder.Author = new()
				{
					Name = authorGuildMember.Username,
					IconUrl = authorGuildMember.AvatarUrl,
					Url = authorGuildMember.AvatarUrl
				};
				_logger.Trace("Author is still in the guild...");
				embedBuilder.Description = $"Tag \"{tagTitle}\" belongs to {authorGuildMember.Mention}.";
			}
			else if (authorDiscordUser != null)
			{
				embedBuilder.Author = new()
				{
					Name = authorDiscordUser.Username,
					IconUrl = authorDiscordUser.AvatarUrl,
					Url = authorDiscordUser.AvatarUrl
				};
				_logger.Trace("Author is no longer in the guild, but the user still exists...");
				embedBuilder.Description = $"Tag \"{tagTitle}\" belongs to {authorDiscordUser.Mention}, however they are not currently present in the guild. This means you can claim the tag. See `>>help tag claim` for more information.";
			}
			else
			{
				embedBuilder.Author = new()
				{
					Name = "Unknown",
					IconUrl = context.User.DefaultAvatarUrl,
					Url = context.User.DefaultAvatarUrl
				};
				_logger.Trace("Author is not in the cache... Assuming they still exist despite not being in the guild...");
				embedBuilder.Description = $"Tag \"{tagTitle}\" is owned by <@{tagAuthor}> ({tagAuthor}), however they are not currently present in the guild. This means you can claim the tag. See `>>help tag claim` for more information.";
			}
			_logger.Trace("Sending embed...");
			_ = Program.SendMessage(context, embedBuilder.Build());
			_logger.Trace("Embed sent!");
		}

		[Command("claim"), Description("Claims a tag."), RequireGuild, TagCheck(false, TagType.Tag, TagState.Exists)]
		public async Task Claim(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages)))
			{
				_logger.Trace($"Changing ownership of tag \"{tagTitle}\"...");
				Program.Database.Tags.Claim(context.Guild.Id, tagTitle, context.User.Id);
				_ = Program.SendMessage(context, $"{context.User.Mention} has forcefully claimed tag \"{tagTitle}\" using their admin powers.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
			else
			{
				_logger.Trace($"Testing if the current owner of tag \"{tagTitle}\" is still in the guild...");
				ulong tagAuthor = Program.Database.Tags.GetAuthor(context.Guild.Id, tagTitle).Value;
				DiscordMember guildAuthor = await context.Guild.GetMemberAsync(tagAuthor);
				if (guildAuthor != null)
				{
					_logger.Trace($"The owner of tag \"{tagTitle}\" is still in the guild...");
					_ = Program.SendMessage(context, "**[Denied: Tag author is still in the guild!]**");
					_logger.Trace("Message sent!");
				}
				else
				{
					_logger.Trace($"The owner of tag \"{tagTitle}\" is no longer in the guild \"{context.Guild.Id}\", changing ownership...");
					Program.Database.Tags.Claim(context.Guild.Id, tagTitle, context.User.Id);
					_ = Program.SendMessage(context, $"Due to the old tag author <@{tagAuthor}> ({tagAuthor}), leaving, the tag \"{tagTitle}\" has been transferred to {context.User.Mention}", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
					_logger.Trace("Message sent!");
				}
			}
		}

		[Command("transfer"), Description("Transfers tag ownership to another person."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Transfer(CommandContext context, string tagTitle, DiscordUser newAuthor)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace("Checking if executing user is a staff member...");
			if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages)))
			{
				_logger.Trace($"User is indeed staff. Changing ownership of tag \"{tagTitle}\"...");
				Program.Database.Tags.Claim(context.Guild.Id, tagTitle, newAuthor.Id);
				_ = Program.SendMessage(context, $"{context.User.Mention} forcefully transferred tag \"{tagTitle}\" to {newAuthor.Mention} using their admin powers.", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
			else
			{
				_logger.Trace($"User is not staff, but they are the owner of tag \"{tagTitle}\"... Checking if the new owner is in the guild...");
				DiscordMember newAuthorMember = await context.Guild.GetMemberAsync(newAuthor.Id);
				if (newAuthorMember == null)
				{
					_logger.Trace("The new owner is not in the guild...");
					_ = Program.SendMessage(context, $"**[Denied: {newAuthor} isn't in the guild.]**", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
					_logger.Trace("Message sent!");
				}
				else
				{
					_logger.Trace("The new owner is in the guild. Transfer tag over...");
					Program.Database.Tags.Claim(context.Guild.Id, tagTitle, newAuthor.Id);
					_ = Program.SendMessage(context, $"Due to the old tag author, {context.User.Mention}, willing letting go of tag \"{tagTitle}\", ownership has now been transferred to {newAuthor.Mention}", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
					_logger.Trace("Message sent!");
				}
			}
		}

		[Command("get_aliases"), Description("Gets all aliases of a tag."), RequireGuild, TagCheck(false, TagType.Tag, TagState.Exists), Aliases("getaliases")]
		public async Task AllAliases(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Getting all aliases for tag \"{tagTitle}\"...");
			string[] userTags = Program.Database.Tags.GetAliases(context.Guild.Id, tagTitle) ?? Array.Empty<string>();
			if (userTags.Length == 0)
			{
				_logger.Trace("No aliases found...");
				_ = Program.SendMessage(context, $"No aliases found for tag \"{tagTitle}\"");
				_logger.Trace("Message sent!");
				return;
			}
			_logger.Trace("Creating embed...");
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.User.Username,
				IconUrl = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl,
				Url = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All aliases for tag \"{tagTitle}\"";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			_logger.Trace("Sending embed...");
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", userTags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
			_logger.Trace("Embed sent!");
		}

		[Command("user"), Description("Lists all the tags a person owns on the server."), RequireGuild]
		public async Task UserTags(CommandContext context, DiscordUser user)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace($"Getting all tags that user {user.Id} created...");
			string[] userTags = Program.Database.Tags.GetUser(context.Guild.Id, user.Id) ?? Array.Empty<string>();

			if (userTags.Length == 0)
			{
				_logger.Trace("No tags found...");
				_ = Program.SendMessage(context, "No tags found!");
				_logger.Trace("Message sent!");
				return;
			}

			_logger.Trace("Creating embed...");
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.User.Username,
				IconUrl = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl,
				Url = context.User.AvatarUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All tags by {user.Username} on \"{context.Guild.Name}\"";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			_logger.Trace("Sending embed...");
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", userTags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
			_logger.Trace("Embed sent!");
		}

		[Command("user"), Description("Lists all the tags for the current user on the server."), RequireGuild]
		public async Task UserTags(CommandContext context) => await UserTags(context, context.User);

		[Command("all"), Description("Lists all the tags in this server."), RequireGuild]
		public async Task All(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace("Getting all tags in the server...");
			string[] allTags = Program.Database.Tags.GetGuild(context.Guild.Id);

			if (allTags[0] == null)
			{
				_logger.Trace("No tags found...");
				_ = Program.SendMessage(context, "No tags found!");
				_logger.Trace("Message sent!");
				return;
			}

			_logger.Trace("Creating embed...");
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All tags for \"{context.Guild.Name}\"";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			_logger.Trace("Sending embed...");
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", allTags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
			_logger.Trace("Embed sent!");
		}

		[Command("realname"), Description("Gets the original tag using an alias."), Aliases("real_name"), RequireGuild, TagCheck(false, TagType.Alias, TagState.Exists)]
		public async Task RealName(CommandContext context, string tagTitle)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			_logger.Trace($"Testing if tag \"{tagTitle}\" is an alias or a tag...");
			if (!Program.Database.Tags.IsAlias(context.User.Id, tagTitle).Value)
			{
				_logger.Trace($"Tag \"{tagTitle}\" is a tag...");
				_ = Program.SendMessage(context, $"\"{tagTitle}\" is the original tag!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
			}
			else
			{
				_logger.Trace($"Tag \"{tagTitle}\" is an alias...");
				_ = Program.SendMessage(context, $"The original tag for the alias \"{tagTitle}\" is `>>tag {Program.Database.Tags.RealName(context.Guild.Id, tagTitle)}`", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				_logger.Trace("Message sent!");
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
			else if (RequireOwner && (Program.Database.Tags.GetAuthor(context.Guild.Id, tagTitle) != context.User.Id || context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))))
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
