using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Npgsql;
using Tomoe.Utils;
using DSharpPlus.Interactivity;
using Tomoe.Database.Interfaces;

namespace Tomoe.Commands.Public
{
	[Group("remind")]
	[Aliases("reminders")]
	[Description("Creates a reminder to go off at the specified time.")]
	public class Reminders : BaseCommandModule
	{
		private static readonly Logger Logger = new Logger("Commands/Public/Tasks");

		[GroupCommand]
		public async Task Create(CommandContext context, TimeSpan setOff, [RemainingText] string content)
		{
			Program.Database.Assignments.Create(AssignmentType.Reminder, context.Guild.Id, context.Channel.Id, context.Message.Id, context.User.Id, DateTime.Now + setOff, DateTime.Now, content);
			_ = Program.SendMessage(context, $"Set off at {DateTime.Now.Add(setOff).ToString("MMM dd', 'HHH':'mm':'ss")}: ```\n{content}```", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
		}

		[GroupCommand]
		public async Task ListByGroup(CommandContext context) => await List(context);

		[Command("list")]
		[Description("Lists what reminders are set.")]
		public async Task List(CommandContext context)
		{
			Assignment[] tasks = Program.Database.Assignments.SelectAllReminders(context.User.Id);
			List<string> reminders = new List<string>();
			if (tasks == null)
			{
				reminders.Add("No reminders are set!");
			}
			else
			{
				Logger.Trace(tasks.Length.ToString());
				foreach (Assignment task in tasks)
				{
					Logger.Trace($"Id #{task.TaskId}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss")}: {task.Content}");
					reminders.Add($"Id #{task.TaskId}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss")}: {task.Content}");
				}
			}

			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All reminders for {context.Member.GetCommonName()}";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join("\n", reminders.ToArray()), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
		}

		[Command("remove")]
		[Description("Removes a reminder.")]
		public async Task Remove(CommandContext context, int taskId)
		{
			Logger.Trace("Executing remove");
			Assignment? task = Program.Database.Assignments.Select(context.User.Id, AssignmentType.Reminder, taskId);
			if (!task.HasValue)
			{
				_ = Program.SendMessage(context, $"Reminder #{taskId} does not exist!");
			}
			else
			{
				Program.Database.Assignments.Remove(taskId);
				_ = Program.SendMessage(context, $"Reminder #{taskId} was removed!");
			}
		}

		[Command("remove")]
		public async Task Remove(CommandContext context, string taskId)
		{
			try
			{
				_ = Remove(context, int.Parse(taskId.Remove(0, 1)));
			}
			catch (FormatException)
			{
				_ = Program.SendMessage(context, $"\"{taskId}\" is not an id.");
			}
		}

		public static void StartRoutine()
		{
			Timer timer = new();
			timer.Interval = 1000;
			timer.Elapsed += async (object sender, ElapsedEventArgs e) =>
			{
				Assignment[] tasks;
				try
				{
					tasks = Program.Database.Assignments.SelectAllAssignments();
				}
				catch (NpgsqlOperationInProgressException)
				{
					return;
				}
				if (tasks == null)
				{
					return;
				}
				foreach (Assignment task in tasks)
				{
					if (task.SetOff.CompareTo(DateTime.Now) < 0)
					{
						CommandsNextExtension commandsNext = (await Program.Client.GetCommandsNextAsync())[0];
						DiscordGuild guild = await Program.Client.ShardClients[0].GetGuildAsync(task.GuildId);
						DiscordChannel channel = guild.GetChannel(task.ChannelId);
						CommandContext context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Utils.Config.Prefix, commandsNext.RegisteredCommands["remind"], null);
						switch (task.TaskType)
						{
							case AssignmentType.Reminder:
								_ = Program.SendMessage(context, $"Set at {task.SetAt.ToString("MMM dd', 'HHH':'mm")}: {task.Content}", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
								Program.Database.Assignments.Remove(task.TaskId);
								break;
							case AssignmentType.TempBan:
								await Moderation.Unban.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								Program.Database.Assignments.Remove(task.TaskId);
								break;
							case AssignmentType.TempMute:
								await Moderation.Unmute.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								Program.Database.Assignments.Remove(task.TaskId);
								break;
						}
					}
				}
			};
			timer.Start();
		}
	}
}
