using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Public
{
	[Group("assignment"), Aliases("reminders", "remind", "task"), Description("Creates an assignment to go off at the specified time.")]
	public class Assignments : BaseCommandModule
	{
		public Database Database { private get; set; }
		internal static readonly List<Assignment> AssignmentsList = new();
		private static readonly Timer _localTimer = new();
		private static readonly Timer _databaseTimer = new();

		[GroupCommand, Aliases("create")]
		public async Task Create(CommandContext context, TimeSpan setOff, [RemainingText] string content = "**[Notice: No description was set!]**") => await Create(context, DateTime.UtcNow + setOff, content);

		[GroupCommand, Aliases("create")]
		public async Task Create(CommandContext context, DateTime setOff, [RemainingText] string content = "**[Notice: No description was set!]**")
		{
			// If the reminder's set off is before now.
			DateTime now = DateTime.UtcNow;
			if (setOff <= now)
			{
				_ = await Program.SendMessage(context, "I can't go back in time and tell you this!");
				return;
			}

			Assignment assignment = new();
			assignment.Type = AssignmentType.Reminder;
			assignment.ChannelId = context.Channel.Id;
			assignment.Content = content;
			assignment.MessageId = context.Message.Id;
			assignment.SetAt = DateTime.UtcNow;
			assignment.SetOff = setOff;
			assignment.UserId = context.User.Id;
			assignment.Jumplink = context.Message.JumpLink;
			assignment.IsDm = context.Channel.IsPrivate;
			assignment.GuildId = context.Guild?.Id ?? 0;

			// If the assignment setOff is within 30 minutes from now, keep track of it locally.
			// Otherwise put it into the database.
			if (setOff < now.AddMinutes(30))
			{
				AssignmentsList.Add(assignment);
			}
			else
			{
				_ = Database.Assignments.Add(assignment);
				_ = await Database.SaveChangesAsync();
			}
			_ = await Program.SendMessage(context, $"#{assignment.Id}, In {assignment.SetOff.Humanize()}: {assignment.Content}");
		}

		[GroupCommand, Aliases("list", "upcoming")]
		public async Task List(CommandContext context)
		{
			// Merge local and database assignments into one list.
			List<Assignment> assignments = AssignmentsList.Where(assignment => assignment.UserId == context.User.Id).ToList();
			assignments.AddRange(Database.Assignments.Where(assignment => assignment.UserId == context.User.Id));

			DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Reminders for {context.User.Username}");
			StringBuilder stringBuilder = new();
			foreach (Assignment assignment in assignments)
			{
				_ = stringBuilder.Append($"{(assignment.IsStatic ? "Annual" : null)} #{assignment.Id}, About {assignment.SetOff.Humanize()}: {assignment.Content}\n\n");
			}

			InteractivityExtension interactivity = context.Client.GetInteractivity();
			string reminders = stringBuilder.ToString();
			Page[] pages = interactivity.GeneratePagesInEmbed(reminders == string.Empty ? "No reminders set!" : reminders, SplitType.Line, discordEmbedBuilder).ToArray();
			if (pages.Length == 1)
			{
				_ = await Program.SendMessage(context, null, pages[0].Embed);
			}
			else
			{
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
		}

		[Command("annual"), Aliases("static")]
		public async Task Annual(CommandContext context, TimeSpan timeSpan, [RemainingText] string content)
		{
			// If the reminder's set off is before now.
			if (timeSpan <= TimeSpan.FromDays(1))
			{
				_ = await Program.SendMessage(context, "Annual reminders can only occur once a day. Try setting your reminder to a week or month.");
				return;
			}

			Assignment assignment = new();
			assignment.Type = AssignmentType.Reminder;
			assignment.ChannelId = context.Channel.Id;
			assignment.Content = content;
			assignment.MessageId = context.Message.Id;
			assignment.SetAt = DateTime.UtcNow;
			assignment.SetOff = DateTime.UtcNow + timeSpan;
			assignment.UserId = context.User.Id;
			assignment.Jumplink = context.Message.JumpLink;
			assignment.IsDm = context.Channel.IsPrivate;
			assignment.GuildId = context.Guild?.Id ?? 0;
			assignment.IsStatic = true;
			assignment.Annuality = timeSpan;

			_ = Database.Assignments.Add(assignment);
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Annual #{assignment.Id}, About {assignment.SetOff.Humanize()}: {assignment.Content}");
		}

		[Command("info")]
		public async Task Info(CommandContext context, Assignment assignment)
		{
			DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Info on Reminder #{assignment.Id}");
			StringBuilder stringBuilder = new();
			_ = stringBuilder.Append($"Id: {assignment.Id}\n\n");
			_ = stringBuilder.Append($"Jumplink: {assignment.Jumplink}\n\n");
			_ = stringBuilder.Append($"Set At: {assignment.SetAt.Humanize()}\n\n");
			_ = stringBuilder.Append($"Set Off: {assignment.SetOff.Humanize()}\n\n");
			_ = stringBuilder.Append($"Is Dm: {assignment.IsDm}\n\n");
			_ = stringBuilder.Append($"Is Annual: {assignment.IsStatic}\n\n");
			discordEmbedBuilder.Description = stringBuilder.ToString();
			_ = discordEmbedBuilder.AddField("Content:", assignment.Content);
			_ = await Program.SendMessage(context, null, discordEmbedBuilder.Build());
		}

		[Command("drop"), Aliases("remove")]
		public async Task Drop(CommandContext context, Assignment assignment)
		{
			// Try removing the assignment from the local list. If it isn't there, then try removing it from the database.
			if (!AssignmentsList.Remove(assignment))
			{
				_ = Database.Assignments.Remove(assignment);
				_ = await Database.SaveChangesAsync();
			}
			_ = await Program.SendMessage(context, $"Assignment #{assignment.Id} was sucessfully removed!");
		}

		[Command("clear"), Aliases("removeall", "remove_all")]
		public async Task ClearAll(CommandContext context)
		{
			foreach (Assignment assignment in AssignmentsList)
			{
				if (assignment.UserId == context.User.Id)
				{
					_ = AssignmentsList.Remove(assignment);
				}
			}
			Database.Assignments.RemoveRange(Database.Assignments.Where(assignment => assignment.UserId == context.User.Id));
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, "Cleared all reminders!");
		}

		public static Task StartRoutine(DiscordClient _client, GuildDownloadCompletedEventArgs _eventArgs)
		{
			_localTimer.Elapsed += LocalTimerElapsed;
			_localTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
			_localTimer.Start();
			_databaseTimer.Elapsed += DatabaseTimerElapsed;
			_databaseTimer.Interval = TimeSpan.FromMinutes(30).TotalMilliseconds;
			_databaseTimer.Start();
			DatabaseTimerElapsed();
			return Task.CompletedTask;
		}

		public static async void LocalTimerElapsed(object sender = null, ElapsedEventArgs eventArgs = null)
		{
			List<Assignment> assignments = AssignmentsList.Where(assignment => assignment.SetOff <= DateTime.UtcNow).ToList();
			await HandleAssignments(assignments);
			assignments.ForEach(assignment => AssignmentsList.Remove(assignment));
		}

		public static async void DatabaseTimerElapsed(object sender = null, ElapsedEventArgs eventArgs = null)
		{
			IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			List<Assignment> assignments = await database.Assignments.Where(assignment => assignment.SetOff <= DateTime.UtcNow).ToListAsync();
			await HandleAssignments(assignments);
			// Filter out annual assignments
			List<Assignment> staticAssignments = assignments.Where(assignment => assignment.IsStatic).ToList();
			staticAssignments.ForEach(assignment => assignment.SetOff += assignment.Annuality);
			database.Assignments.RemoveRange(assignments.Except(staticAssignments));
			_ = await database.SaveChangesAsync();
		}

		public static async Task HandleAssignments(List<Assignment> assignments)
		{
			if (assignments.Count == 0) return;
			DiscordClient client = Program.Client.ShardClients[0];
			foreach (Assignment assignment in assignments)
			{
				try
				{
					DiscordGuild discordGuild = null;
					DiscordChannel discordChannel = null;
					DiscordUser discordUser = await client.GetUserAsync(assignment.UserId);

					try
					{
						discordGuild = await client.GetGuildAsync(assignment.GuildId, false);
						if (assignment.IsDm)
						{
							discordChannel = await (await discordUser.Id.GetMember(discordGuild)).CreateDmChannelAsync();
						}
						else
						{
							discordChannel = discordGuild.GetChannel(assignment.ChannelId);
						}
					}
					catch (NotFoundException)
					{
						continue;
					}

					switch (assignment.Type)
					{
						case AssignmentType.Tempban:
							await Moderation.Unban.ByProgram(discordGuild, discordUser, assignment.Jumplink, "Tempban complete!");
							break;
						case AssignmentType.Tempmute:
							await Moderation.Unmute.ByProgram(discordGuild, discordUser, assignment.Jumplink, "Tempmute complete!");
							break;
						case AssignmentType.Tempantimeme:
							await Moderation.Unantimeme.ByProgram(discordGuild, discordUser, assignment.Jumplink, "Tempantimeme complete!");
							break;
						case AssignmentType.Tempvoiceban:
							await Moderation.Unvoiceban.ByProgram(discordGuild, discordUser, assignment.Jumplink, "Tempvoiceban complete!");
							break;
						case AssignmentType.Reminder:
							if (assignment.IsDm)
							{
								_ = await discordChannel.SendMessageAsync($"{(assignment.IsStatic ? "Annual reminder" : assignment.SetAt.Humanize())}: {assignment.Content}\nContext: {assignment.Jumplink}");
							}
							else
							{
								CommandsNextExtension commandsNext = client.GetCommandsNext();
								CommandContext context = commandsNext.CreateContext(await discordChannel.GetMessageAsync(assignment.MessageId), Program.Config.DiscordBotPrefix, commandsNext.RegisteredCommands["reminders"]);
								_ = await Program.SendMessage(context, $"{(assignment.IsStatic ? "Annual reminder" : assignment.SetAt.Humanize())}: {assignment.Content}\nContext: {assignment.Jumplink}");
							}
							break;
						default: break;
					}
				}
				// Discord fucked up, put the assignment back into the database 30 minutes later.
				catch (ServerErrorException)
				{
					using IServiceScope scope = Program.ServiceProvider.CreateScope();
					using Database database = scope.ServiceProvider.GetService<Database>();
					assignment.SetOff += TimeSpan.FromMinutes(30);
					_ = database.Assignments.Add(assignment);
					_ = await database.SaveChangesAsync();
				}
				// Someone has dm's off. Remove the assignment as we can just assume they won't turn dm's back on.
				catch (UnauthorizedException)
				{
					continue;
				}
			}
		}

		public static async Task Dispose()
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			using Database database = scope.ServiceProvider.GetService<Database>();
			_databaseTimer.Dispose();
			_localTimer.Dispose();
			database.Assignments.AddRange(AssignmentsList);
			_ = await database.SaveChangesAsync();
		}
	}
}
