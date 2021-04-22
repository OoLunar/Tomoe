using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("modlog"), Description("Logs something to the modlog."), Aliases("mod_log", "log", "ml"), RequireUserPermissions(Permissions.ManageMessages)]
	public class ModLogs : BaseCommandModule
	{
		[GroupCommand]
		public async Task Overload(CommandContext context, string action, [RemainingText] string reason = Constants.MissingReason)
		{
			await Record(context, action, reason);
			_ = await Program.SendMessage(context, "Successfully recorded event into the ModLog.");
		}

		public static async Task Record(ulong guildId, string action, [RemainingText] string reason = Constants.MissingReason)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			ModLog modLog = new();
			modLog.Action = action;
			modLog.GuildId = guildId;
			modLog.LogId = database.ModLogs.Where(log => log.GuildId == guildId).Count() + 1;
			modLog.Reason = reason;
			_ = database.ModLogs.Add(modLog);
			_ = await database.SaveChangesAsync();
		}

		public static async Task Record(CommandContext context, string action, [RemainingText] string reason = Constants.MissingReason)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			ModLog modLog = new();
			modLog.Action = action;
			modLog.LogId = database.ModLogs.Where(log => log.GuildId == context.Guild.Id).Count() + 1;
			modLog.GuildId = context.Guild.Id;
			modLog.ChannelId = context.Channel.Id;
			modLog.MessageId = context.Message.Id;
			modLog.JumpLink = context.Message.JumpLink.ToString();
			modLog.Reason = reason;
			_ = database.ModLogs.Add(modLog);
			_ = await database.SaveChangesAsync();
		}

		public static async Task Record(CommandContext context)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			CommandUsage command = new();
			command.Command = context.Message.Content;
			command.JumpLink = context.Message.JumpLink.ToString();
			command.Executer = $"{context.User.Username}#{context.User.Discriminator}";
			command.ExecuterId = context.User.Id;

			_ = database.CommandUsages.Add(command);
			_ = await database.SaveChangesAsync();
		}
	}
}
