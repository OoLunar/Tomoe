using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Tomoe.Db;

namespace Tomoe.Commands.Public
{
	[Group("tag"), Description("Sends a pre-specified message.")]
	public class Tags : BaseCommandModule
	{
		public Database Database { private get; set; }

		[GroupCommand]
		public async Task Send(CommandContext context, Tag tag) => await Program.SendMessage(context, tag.Content);

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
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: Tag `{tagName}` already exists!"));
				return;
			}

			tag = new();
			tag.Content = tagContent;
			tag.GuildId = context.Guild.Id;
			tag.Name = tagName.ToLowerInvariant();
			tag.OwnerId = context.User.Id;
			tag.TagId = Database.Tags.Where(tag => tag.GuildId == context.Guild.Id).Count();
			_ = Database.Tags.Add(tag);
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Tag `{tagName}` created!");
		}

		[Command("remove"), Description("Deletes a tag from the guild.")]
		public async Task Remove(CommandContext context, Tag tag)
		{
			// Query the admin roles (server side)
			System.Collections.Generic.List<ulong> list = await Database.GuildConfigs.Where(guildConfig => guildConfig.Id == context.Guild.Id).Select(guildConfig => guildConfig.AdminRoles).FirstOrDefaultAsync();
			// See if the user has any (client side)
			bool isAdmin = list.Intersect(context.Member.Roles.Select(role => role.Id)).Any();
			if (context.Member.HasPermission(Permissions.ManageMessages) || isAdmin || tag.OwnerId == context.User.Id)
			{
				_ = Database.Tags.Attach(tag);
				_ = Database.Tags.Remove(tag);
				_ = await Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, $"Tag `{tag.Name}` has been deleted.");
			}
			else
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: You do not have permission to delete tag `{tag.Name}`"));
			}
		}
	}
}
