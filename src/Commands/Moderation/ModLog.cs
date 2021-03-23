using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

// TODO: List, Sort, Find and Update commands.

namespace Tomoe.Commands.Moderation
{
	[Group("modbook"), Aliases("modlog", "logs", "log"), Description("Gets or adds an entry to the mod book."), RequireGuild, RequireUserPermissions(Permissions.ManageMessages)]
	public class ModBook : BaseCommandModule
	{
		public Database Database { private get; set; }

		[GroupCommand, Aliases("get", "retrieve", "read")]
		public async Task Get(CommandContext context, ModLog modLog)
		{
			string victims = string.Join(", ", modLog.IdsAffected.Select(id => $"<@{id}>"));
			if (string.IsNullOrEmpty(victims)) victims = "None";
			DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"ModLog #{modLog.Id}");
			discordEmbedBuilder.Timestamp = modLog.CreatedAt;
			discordEmbedBuilder.Description = modLog.Type switch
			{
				ModLogType.Moderation => $"<@{modLog.IssuerId}> {(modLog.TempAction ? "temp" : null)}{modLog.Action} {victims}. Reason: {Formatter.BlockCode(modLog.Details)}",
				ModLogType.Discord => $"[{modLog.DiscordEvent}] {modLog.Details}",
				ModLogType.Other => modLog.Details,
				_ => modLog.Details
			};

			_ = discordEmbedBuilder.AddField("Issuer", $"<@{modLog.IssuerId}>", false);
			_ = discordEmbedBuilder.AddField("Victims", victims, false);
			_ = discordEmbedBuilder.AddField("Type", modLog.Type.Humanize());

			if (modLog.Type == ModLogType.Discord)
			{
				_ = discordEmbedBuilder.AddField("Discord Event", modLog.DiscordEvent.Humanize());
			}
			else if (modLog.Type == ModLogType.Moderation)
			{
				_ = discordEmbedBuilder.AddField("Moderation Action", modLog.Action.Humanize());
			}
			_ = await Program.SendMessage(context, null, discordEmbedBuilder.Build());
		}

		public static async Task Add(CommandContext context, string details, DiscordEvent discordEvent = DiscordEvent.None, ModAction modAction = ModAction.Other)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			Assembly entryAssembly = Assembly.GetEntryAssembly();
			string[] sections = entryAssembly.GetTypes().Where(type => type.FullName.StartsWith("Tomoe.Commands", true, CultureInfo.InvariantCulture)).Select(type => type.FullName.Split('.')[2]).Distinct().ToArray();

			ModLog modLog = new();
			modLog.Details = details;
			modLog.DiscordEvent = discordEvent;
			if (context.Command.Module.ModuleType.FullName.ToLowerInvariant() == "tomoe.commands.moderation")
			{
				modLog.Type = ModLogType.Moderation;
			}
			else if (context.Command.Module.ModuleType.FullName.ToLowerInvariant() == "tomoe.commands.listeners" || discordEvent != DiscordEvent.None)
			{
				modLog.Type = ModLogType.Discord;
			}
			else
			{
				modLog.Type = ModLogType.Other;
			}
			modLog.TempAction = context.Command.Name.StartsWith("temp", true, CultureInfo.InvariantCulture);
			modLog.GuildId = context.Guild.Id;
			modLog.IssuerId = context.User.Id;
			modLog.IdsAffected = context.Message.MentionedUsers.Select(user => user.Id).ToArray();
			modLog.Action = modAction;
			_ = database.ModLogs.Add(modLog);
			_ = await database.SaveChangesAsync();
		}
	}
}
