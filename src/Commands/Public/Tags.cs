using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using Tomoe.Db;

namespace Tomoe.Commands.Public
{
	[Group("tag"), Description("Tags are shortcuts for long or confusing pieces of text."), RequireGuild]
	public class Tags : BaseCommandModule
	{
		public Database Database { private get; set; }

		[GroupCommand]
		public async Task Get(CommandContext context, Tag tag) => _ = await Program.SendMessage(context, tag.Content);

		[Command("create")]
		public async Task Create(CommandContext context, string name, [RemainingText] string content)
		{
			CommandsNextExtension commandsNext = context.Client.GetCommandsNext();
			Command command = commandsNext.RegisteredCommands["tag"];
			CommandGroup commandGroup = command as CommandGroup;
			name = name.Trim().ToLowerInvariant();

			// If the name is "tag", any of the aliases of the tag command, or is the name of any subcommands, or is the name of any subcommand aliases.
			if (command.Name == name || command.Aliases.Contains(name) || commandGroup.Children.Select(child => child.Name == name || child.Aliases.Contains(name)) != null)
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error: Tag name {Formatter.InlineCode(name)} is an invalid name.]"));

			Tag tag = new();
			tag.Content = content.Trim();
			tag.GuildId = context.Guild.Id;
			tag.IsAlias = false;
			tag.Name = name.ToLowerInvariant();
			tag.OwnerId = context.User.Id;
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null) _ = await Program.SendMessage(context, Formatter.Bold("[Error: Failed to create tag, guild is not in the database!]"));
			else
			{
				guild.Tags.Add(tag);
				_ = await Program.SendMessage(context, $"Tag {Formatter.InlineCode(tag.Name)} successfully created!");
			}
		}

		[Command("edit")]
		public async Task Edit(CommandContext context, Tag tag, [RemainingText] string content)
		{
			if (await context.Member.IsAdmin(context.Guild) || context.Member.HasPermission(Permissions.ManageMessages) || tag.OwnerId == context.User.Id)
			{
				Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
				if (guild == null) _ = await Program.SendMessage(context, Formatter.Bold("[Error: Failed to edit tag, guild is not in the database!]"));
				else
				{
					tag.Content = content;
					// Since an entity isn't tracked once it leaves the scope, I'm not sure how else to update the tag.
					_ = guild.Tags.Remove(tag);
					guild.Tags.Add(tag);
					_ = await Program.SendMessage(context, $"Tag {Formatter.InlineCode(tag.Name)} successfully updated!");
				}
			}
			else _ = await Program.SendMessage(context, Formatter.Bold($"[Denied: You don't own tag {Formatter.InlineCode(tag.Name)}!"));
		}

		[Command("delete")]
		public async Task Delete(CommandContext context, Tag tag)
		{
			if (await context.Member.IsAdmin(context.Guild) || context.Member.HasPermission(Permissions.ManageMessages) || tag.OwnerId == context.User.Id)
			{
				Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
				if (guild == null) _ = await Program.SendMessage(context, Formatter.Bold("[Error: Failed to edit tag, guild is not in the database!]"));
				else
				{
					_ = guild.Tags.Remove(tag);
					_ = await Program.SendMessage(context, $"Tag {Formatter.InlineCode(tag.Name)} successfully deleted!");
				}
			}
			else _ = await Program.SendMessage(context, Formatter.Bold($"[Denied: You don't own tag {Formatter.InlineCode(tag.Name)}!"));
		}
	}
}
