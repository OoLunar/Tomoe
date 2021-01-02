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
using DSharpPlus;

namespace Tomoe.Commands.Public
{
	[Group("remind"), Aliases("reminders"), Description("Creates a reminder to go off at the specified time.")]
	public class Reminders : BaseCommandModule
	{
		private static readonly Logger Logger = new Logger("Commands.Public.Tasks");

		[GroupCommand]
		public async Task Create(CommandContext context, TimeSpan setOff, [RemainingText] string content)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Trace($"Creating reminder for {context.User.Username}");
			Program.Database.Assignments.Create(AssignmentType.Reminder, context.Guild.Id, context.Channel.Id, context.Message.Id, context.User.Id, DateTime.Now + setOff, DateTime.Now, content);
			Logger.Trace("Reminder created!");
			_ = Program.SendMessage(context, $"Set off at {DateTime.Now.Add(setOff).ToString("MMM dd', 'HHH':'mm':'ss")}: ```\n{content}```", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
			Logger.Trace("Message sent!");
		}

		[GroupCommand]
		public async Task ListByGroup(CommandContext context) => await List(context);

		[Command("list")]
		[Description("Lists what reminders are set.")]
		public async Task List(CommandContext context)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Trace($"Retriving reminders for {context.User.Username}...");
			Assignment[] tasks = Program.Database.Assignments.SelectAllReminders(context.User.Id);
			List<string> reminders = new();
			if (tasks == null)
			{
				Logger.Trace("No reminders found...");
				reminders.Add("No reminders are set!");
			}
			else
			{
				Logger.Trace($"Found a total of {tasks.Length} tasks...");
				foreach (Assignment task in tasks)
				{
					Logger.Trace($"Id #{task.AssignmentId}, Set off at {task.SetOff}");
					reminders.Add($"Id #{task.AssignmentId}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss")}: {task.Content}");
				}
			}

			Logger.Trace("Creating embed...");
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			embedBuilder.Title = $"All reminders for {context.Member.GetCommonName()}";
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			Logger.Trace("Sending embed...");
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join("\n", reminders.ToArray()), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
			Logger.Trace("Embed sent!");
		}

		[Command("remove"), Description("Removes a reminder.")]
		public async Task Remove(CommandContext context, int taskId)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Trace($"Selecting reminder #{taskId}...");
			Assignment? task = Program.Database.Assignments.Retrieve(context.User.Id, AssignmentType.Reminder, taskId);
			if (!task.HasValue)
			{
				Logger.Trace($"Reminder #{taskId} doesn't exist!");
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
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			try
			{
				Logger.Trace($"Attempting to parse {taskId}...");
				_ = Remove(context, int.Parse(taskId.Remove(0, 1)));
			}
			catch (FormatException)
			{
				Logger.Trace($"{taskId} is not an id.");
				_ = Program.SendMessage(context, $"\"{taskId}\" is not an id.");
			}
		}

		public static void StartRoutine()
		{
			Logger.Debug("Starting reminders routine...");
			Timer timer = new();
			timer.Interval = 1000;
			Logger.Trace("Setting reminders routine to go off every 1 second...");
			timer.Elapsed += async (object sender, ElapsedEventArgs e) =>
			{
				Assignment[] tasks;
				try
				{
					Logger.Trace("Selecting all assignments...");
					tasks = Program.Database.Assignments.SelectAllAssignments();
				}
				catch (NpgsqlOperationInProgressException)
				{
					Logger.Trace("Still selecting from previous iteration!");
					return;
				}
				if (tasks == null)
				{
					Logger.Trace("No reminders found!");
					return;
				}
				foreach (Assignment task in tasks)
				{
					if (task.SetOff.CompareTo(DateTime.Now) < 0)
					{
						Logger.Trace($"Reminder #{task.AssignmentId} is due to go off!");
						DiscordClient client = Program.Client.ShardClients[0];
						CommandsNextExtension commandsNext = client.GetCommandsNext();
						Logger.Trace($"Getting #{task.AssignmentId}'s guild...");
						DiscordGuild guild = await client.GetGuildAsync(task.GuildId);
						Logger.Trace($"Getting #{task.AssignmentId}'s channel...");
						DiscordChannel channel = guild.GetChannel(task.ChannelId);
						CommandContext context;
						switch (task.AssignmentType)
						{
							case AssignmentType.Reminder:
								Logger.Trace($"Creating reminder context for #{task.AssignmentId}...");
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Utils.Config.Prefix, commandsNext.RegisteredCommands["remind"], null);
								_ = Program.SendMessage(context, $"Set at {task.SetAt.ToString("MMM dd', 'HHH':'mm")}: {task.Content}", ExtensionMethods.FilteringAction.CodeBlocksIgnore);
								Logger.Trace("Message sent!");
								Program.Database.Assignments.Remove(task.AssignmentId);
								Logger.Trace("Reminder removed!");
								break;
							case AssignmentType.TempBan:
								Logger.Trace($"Creating tempban context for #{task.AssignmentId}...");
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Utils.Config.Prefix, commandsNext.RegisteredCommands["tempban"], null);
								await Moderation.Unban.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								Program.Database.Assignments.Remove(task.AssignmentId);
								Logger.Trace("Unban removed!");
								break;
							case AssignmentType.TempMute:
								Logger.Trace($"Creating reminder context for #{task.AssignmentId}...");
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Utils.Config.Prefix, commandsNext.RegisteredCommands["tempmute"], null);
								await Moderation.Unmute.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								Program.Database.Assignments.Remove(task.AssignmentId);
								Logger.Trace("Unmute removed!");
								break;
						}
					}
				}
			};
			timer.Start();
		}
	}
}
