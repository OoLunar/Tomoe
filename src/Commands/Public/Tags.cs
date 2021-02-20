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

using Tomoe.Utils;
using Tomoe.Db;

namespace Tomoe.Commands.Public
{
	[Group("tag"), Description("Gets a tag's content."), RequireGuild]
	public class Tags : BaseCommandModule
	{
		[GroupCommand]
		public async Task Get(CommandContext context, [RemainingText] string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			if (tagTitle.Length > 32) _ = await Program.SendMessage(context, Constants.Tags.TooLong);
			else _ = await Program.SendMessage(context, Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id).Tags.Select(tag => tag.Name == tagTitle ? tag.Content : (tag.Aliases.Contains(tagTitle) ? tag.OriginalTag.Content : Formatter.Bold($"[Error: \"{tagTitle}\" doesn't exist!]"))).First());
		}

		[Command("create"), Description("Creates a tag."), RequireGuild, TagCheck(false, TagType.Any, TagState.Missing)]
		public async Task Create(CommandContext context, string tagTitle, [RemainingText] string content)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Tag tag = new();
			tag.Content = content.Trim();
			tag.GuildId = context.Guild.Id;
			tag.Name = tagTitle;
			tag.OwnerId = context.User.Id;
			tag.Uses = 0;
			Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id).Tags.Add(tag);

			_ = await Program.SendMessage(context, $"Tag \"{tagTitle}\" has been created!");
		}

		[Command("edit"), Description("Edits a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Edit(CommandContext context, string tagTitle, [RemainingText] string content)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();

			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			tag.Content = content.Trim();

			_ = await Program.SendMessage(context, $"Tag \"{tagTitle}\" successfully edited.");
		}

		[Command("delete"), Description("Deletes a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Delete(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();

			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			_ = guild.Tags.Remove(guild.Tags.First(tag => tag.Name == tagTitle));

			_ = await Program.SendMessage(context, $"Tag \"{tagTitle}\" successfully deleted.");
		}

		[Command("alias"), Description("Creates an alias for a tag."), RequireGuild, TagCheck(false, TagType.Any, TagState.Missing)]
		public async Task Alias(CommandContext context, string newName, string oldName)
		{
			newName = newName.Trim().ToLowerInvariant();
			oldName = oldName.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == oldName);

			if (tag != null)
			{
				_ = await Program.SendMessage(context, $"Tag \"{oldName}\" does not exist!");
			}
			else if (tag.OriginalTag != null)
			{
				_ = await Program.SendMessage(context, Constants.Tags.AliasesOfAliases);
			}
			else
			{
				tag.Aliases.Add(newName);
				_ = await Program.SendMessage(context, $"Tag \"{newName}\" has been created!");
			}
		}

		[Command("delete_alias"), Description("Deletes a tag alias."), RequireGuild, TagCheck(true, TagType.Alias, TagState.Exists)]
		public async Task DeleteAlias(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Aliases.Contains(tagTitle));
			_ = tag.Aliases.Remove(tagTitle);
			_ = await Program.SendMessage(context, $"Tag alias \"{tagTitle}\" successfully deleted.");
		}

		[Command("delete_all_aliases"), Description("Deletes all aliases for a tag."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task DeleteAllAliases(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Aliases.Contains(tagTitle));
			tag.Aliases = new();
			_ = await Program.SendMessage(context, $"All aliases for \"{tagTitle}\" have been removed!");
		}

		[Command("exist"), Description("Tests if a tag exists."), RequireGuild, TagCheck(false, TagType.Any, TagState.Irrelevant)]
		public async Task Exist(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);

			if (tag != null) _ = await Program.SendMessage(context, $"\"{tagTitle}\" doesn't exist!");
			else _ = await Program.SendMessage(context, $"Tag \"{tagTitle}\" does exist!");
		}

		[Command("is_alias"), Description("Tests if a tag is an alias."), RequireGuild, TagCheck(false, TagType.Any, TagState.Exists)]
		public async Task IsAlias(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			_ = await Program.SendMessage(context, $"Tag \"{tagTitle}\" is {(tag.OriginalTag == null ? null : "not")} an alias.");
		}

		[Command("get_author"), Description("Gets the author of a tag."), RequireGuild, Aliases("getauthor", "author"), TagCheck(false, TagType.Any, TagState.Exists)]
		public async Task GetAuthor(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			ulong tagAuthor = tag.OwnerId;
			DiscordUser authorDiscordUser = await context.Client.GetUserAsync(tagAuthor);
			DiscordMember authorGuildMember = await context.Guild.GetMemberAsync(tagAuthor);
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, tagTitle);
			embedBuilder.Title = tagTitle;
			if (authorGuildMember != null)
			{
				embedBuilder.Author = new()
				{
					Name = authorGuildMember.Username,
					IconUrl = authorGuildMember.AvatarUrl,
					Url = authorGuildMember.AvatarUrl
				};
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
				embedBuilder.Description = $"Tag \"{tagTitle}\" belongs to {authorDiscordUser.Mention}, however they are not currently present in the guild. This means you can claim the tag. See {Formatter.InlineCode(">>help tag claim")} for more information.";
			}
			else
			{
				embedBuilder.Author = new()
				{
					Name = "Unknown",
					IconUrl = context.User.DefaultAvatarUrl,
					Url = context.User.DefaultAvatarUrl
				};
				embedBuilder.Description = $"Tag \"{tagTitle}\" is owned by <@{tagAuthor}> ({tagAuthor}), however they are not currently present in the guild. This means you can claim the tag. See {Formatter.InlineCode(">>help tag claim")} for more information.";
			}
			_ = await Program.SendMessage(context, null, embedBuilder.Build());
		}

		[Command("claim"), Description("Claims a tag."), RequireGuild, TagCheck(false, TagType.Tag, TagState.Exists)]
		public async Task Claim(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages)))
			{
				tag.OwnerId = context.User.Id;
				_ = await Program.SendMessage(context, $"{context.User.Mention} has forcefully claimed tag \"{tagTitle}\" using their admin powers.");
			}
			else
			{
				ulong tagAuthor = tag.OwnerId;
				DiscordMember guildAuthor = await context.Guild.GetMemberAsync(tagAuthor);
				if (guildAuthor != null)
				{
					_ = await Program.SendMessage(context, Constants.Tags.AuthorStillPresent);
				}
				else
				{
					tag.OwnerId = context.User.Id;
					_ = await Program.SendMessage(context, $"Due to the old tag author <@{tagAuthor}> ({tagAuthor}), leaving, the tag \"{tagTitle}\" has been transferred to {context.User.Mention}");
				}
			}
		}

		[Command("transfer"), Description("Transfers tag ownership to another person."), RequireGuild, TagCheck(true, TagType.Tag, TagState.Exists)]
		public async Task Transfer(CommandContext context, string tagTitle, DiscordUser newAuthor)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			if (context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages)))
			{
				tag.OwnerId = newAuthor.Id;
				_ = await Program.SendMessage(context, $"{context.User.Mention} forcefully transferred tag \"{tagTitle}\" to {newAuthor.Mention} using their admin powers.");
			}
			else
			{
				DiscordMember newAuthorMember = await context.Guild.GetMemberAsync(newAuthor.Id);
				if (newAuthorMember == null)
				{
					_ = await Program.SendMessage(context, Formatter.Bold($"[Denied: {newAuthor} isn't in the guild.]"));
				}
				else
				{
					tag.OwnerId = newAuthor.Id;
					_ = await Program.SendMessage(context, $"Due to the old tag author, {context.User.Mention}, willing letting go of tag \"{tagTitle}\", ownership has now been transferred to {newAuthor.Mention}");
				}
			}
		}

		[Command("get_aliases"), Description("Gets all aliases of a tag."), RequireGuild, TagCheck(false, TagType.Tag, TagState.Exists), Aliases("getaliases")]
		public async Task AllAliases(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			if (tag.Aliases.Count == 0)
			{
				_ = await Program.SendMessage(context, $"No aliases found for tag \"{tagTitle}\"");
				return;
			}
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"All aliases for tag \"{tagTitle}\"");
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", tag.Aliases), SplitType.Character, embedBuilder));
		}

		[Command("user"), Description("Lists all the tags a person owns on the server."), RequireGuild]
		public async Task UserTags(CommandContext context, DiscordUser user)
		{
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			string[] userTags = guild.Tags.Where(tag => tag.OwnerId == user.Id).Select(tag => tag.Name).ToArray();

			if (userTags.Length == 0)
			{
				_ = await Program.SendMessage(context, Constants.Tags.NotFound);
				return;
			}

			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"All tags by {user.Username} on \"{context.Guild.Name}\"");
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", userTags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
		}

		[Command("user"), Description("Lists all the tags for the current user on the server."), RequireGuild]
		public async Task UserTags(CommandContext context) => await UserTags(context, context.User);

		[Command("all"), Description("Lists all the tags in this server."), RequireGuild]
		public async Task All(CommandContext context)
		{
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);

			if (guild.Tags[0] == null)
			{
				_ = await Program.SendMessage(context, Constants.Tags.NotFound);
				return;
			}

			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"All tags for \"{context.Guild.Name}\"");
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join(", ", guild.Tags), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
		}

		[Command("realname"), Description("Gets the original tag using an alias."), Aliases("real_name"), RequireGuild, TagCheck(false, TagType.Alias, TagState.Exists)]
		public async Task RealName(CommandContext context, string tagTitle)
		{
			tagTitle = tagTitle.Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			if (tag.OriginalTag == null) _ = await Program.SendMessage(context, $"\"{tagTitle}\" is the original tag!");
			else _ = await Program.SendMessage(context, $"The original tag for the alias \"{tagTitle}\" is {Formatter.InlineCode($">>tag {tag.OriginalTag.Name}")}");
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
			if (isHelpCommand) return true;
			string tagTitle = context.RawArgumentString.Split(' ')[0].Trim().ToLowerInvariant();
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			Tag tag = guild.Tags.First(tag => tag.Name == tagTitle);
			if (tagTitle.Length > 32)
			{
				_ = await Program.SendMessage(context, Constants.Tags.TooLong);
				return false;
			}
			else if (MustExist == TagState.Exists && tag == null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error: \"{tagTitle}\" doesn't exist!]"));
				return false;
			}
			else if (MustExist == TagState.Missing && tag != null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error: \"{tagTitle}\" already exists!]"));
				return false;
			}
			else if (RequireOwner && (tag.OwnerId != context.User.Id || context.Member.Roles.Any(role => role.Permissions.HasFlag(Permissions.Administrator) || role.Permissions.HasFlag(Permissions.ManageMessages))))
			{
				_ = await Program.SendMessage(context, Constants.Tags.NotOwnerOf);
				return false;
			}
			else if (RequireTagType == TagType.Tag && tag.OriginalTag != null)
			{
				_ = await Program.SendMessage(context, Constants.Tags.NotATag);
				return false;
			}
			else if (RequireTagType == TagType.Alias && tag.OriginalTag == null)
			{
				_ = await Program.SendMessage(context, Constants.Tags.NotAnAlias);
				return false;
			}
			else return true;
		}
	}
}
