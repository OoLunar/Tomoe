using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Npgsql;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	[Group("remind"), Aliases("reminders"), Description("Creates a reminder to go off at the specified time.")]
	public class Reminders : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Tasks");
		internal static readonly Timer Timer = new();

		[GroupCommand]
		public async Task Create(CommandContext context, TimeSpan setOff, [RemainingText] string content)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace($"Creating reminder for {context.User.Username}");
			Program.Database.Assignments.Create(AssignmentType.Reminder, context.Guild.Id, context.Channel.Id, context.Message.Id, context.User.Id, DateTime.Now + setOff, DateTime.Now, content);
			_logger.Trace("Reminder created!");
			_ = await Program.SendMessage(context, $"Set off at {DateTime.Now.Add(setOff).ToString("MMM dd', 'HHH':'mm':'ss", CultureInfo.InvariantCulture)}: ```\n{content}```");
			_logger.Trace("Message sent!");
		}

		[GroupCommand]
		public async Task ListByGroup(CommandContext context) => await List(context);

		[Command("list")]
		[Description("Lists what reminders are set.")]
		public async Task List(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace($"Retriving reminders for {context.User.Username}...");
			Assignment[] tasks = Program.Database.Assignments.SelectAllReminders(context.User.Id);
			List<string> reminders = new();
			if (tasks == null)
			{
				_logger.Trace("No reminders found...");
				reminders.Add("No reminders are set!");
			}
			else
			{
				_logger.Trace($"Found a total of {tasks.Length} tasks...");
				foreach (Assignment task in tasks)
				{
					_logger.Trace($"Id #{task.AssignmentId}, Set off at {task.SetOff}");
					reminders.Add($"Id #{task.AssignmentId}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss", CultureInfo.InvariantCulture)}: {task.Content}");
				}
			}

			_logger.Trace("Creating embed...");
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"All reminders for {context.Member.GetCommonName()}");
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			_logger.Trace("Sending embed...");
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join("\n", reminders.ToArray()), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
			_logger.Trace("Embed sent!");
		}

		[Command("remove"), Description("Removes a reminder.")]
		public async Task Remove(CommandContext context, int taskId)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace($"Selecting reminder #{taskId}...");
			Assignment? task = Program.Database.Assignments.Retrieve(context.User.Id, AssignmentType.Reminder, taskId);
			if (!task.HasValue)
			{
				_logger.Trace($"Reminder #{taskId} doesn't exist!");
				_ = await Program.SendMessage(context, $"**[Error: Reminder #{taskId} does not exist!]**");
			}
			else
			{
				Program.Database.Assignments.Remove(taskId);
				_ = await Program.SendMessage(context, $"Reminder #{taskId} was removed!");
			}
		}

		[Command("remove")]
		public async Task Remove(CommandContext context, string taskId)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			try
			{
				_logger.Trace($"Attempting to parse {taskId}...");
				_ = Remove(context, int.Parse(taskId.Remove(0, 1), CultureInfo.InvariantCulture));
			}
			catch (FormatException)
			{
				_logger.Trace($"{taskId} is not an id.");
				_ = await Program.SendMessage(context, $"\"{taskId}\" is not an id.");
			}
		}

		public static void StartRoutine()
		{
			_logger.Debug("Starting reminders routine...");
			Timer.Interval = 1000;
			_logger.Trace("Setting reminders routine to go off every 1 second...");
			Timer.Elapsed += async (object sender, ElapsedEventArgs e) =>
			{
				Assignment[] tasks;
				try
				{
					_logger.Trace("Selecting all assignments...");
					tasks = Program.Database.Assignments.SelectAllAssignments();
				}
				catch (NpgsqlOperationInProgressException)
				{
					_logger.Trace("Still selecting from previous iteration!");
					return;
				}
				if (tasks == null)
				{
					_logger.Trace("No reminders found!");
					return;
				}
				foreach (Assignment task in tasks)
				{
					if (task.SetOff.CompareTo(DateTime.Now) < 0)
					{
						_logger.Trace($"Reminder #{task.AssignmentId} is due to go off!");
						DiscordClient client = Program.Client.ShardClients[0];
						CommandsNextExtension commandsNext = client.GetCommandsNext();
						_logger.Trace($"Getting #{task.AssignmentId}'s guild...");
						DiscordGuild guild = await client.GetGuildAsync(task.GuildId);
						_logger.Trace($"Getting #{task.AssignmentId}'s channel...");
						DiscordChannel channel = guild.GetChannel(task.ChannelId);
						CommandContext context;
						switch (task.AssignmentType)
						{
							case AssignmentType.Reminder:
								_logger.Trace($"Creating reminder context for #{task.AssignmentId}...");
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["remind"], null);
								_ = await Program.SendMessage(context, $"Set at {task.SetAt.ToString("MMM dd', 'HHH':'mm", CultureInfo.InvariantCulture)}: {task.Content}");
								_logger.Trace("Message sent!");
								Program.Database.Assignments.Remove(task.AssignmentId);
								_logger.Trace("Reminder removed!");
								break;
							case AssignmentType.TempBan:
								_logger.Trace($"Creating tempban context for #{task.AssignmentId}...");
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["tempban"], null);
								await Moderation.Unban.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								Program.Database.Assignments.Remove(task.AssignmentId);
								_logger.Trace("Unban removed!");
								break;
							case AssignmentType.TempMute:
								_logger.Trace($"Creating reminder context for #{task.AssignmentId}...");
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["tempmute"], null);
								await Moderation.Unmute.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								Program.Database.Assignments.Remove(task.AssignmentId);
								_logger.Trace("Unmute removed!");
								break;
							case AssignmentType.TempAntimeme:
								_logger.Trace($"Creating reminder context for #{task.AssignmentId}...");
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["tempantimeme"], null);
								await Moderation.Promeme.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								Program.Database.Assignments.Remove(task.AssignmentId);
								_logger.Trace("Antimeme removed!");
								break;
						}
					}
				}
			};
			Timer.Start();
		}
	}
}
