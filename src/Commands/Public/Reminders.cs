using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Tomoe.Db;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	[Group("remind"), Aliases("reminders"), Description("Creates a reminder to go off at the specified time.")]
	public class Reminders : BaseCommandModule
	{
		internal static readonly Timer Timer = new();

		[GroupCommand]
		public async Task Create(CommandContext context, TimeSpan setOff, [RemainingText] string content)
		{
			Assignment assignment = new();
			assignment.AssignmentType = AssignmentType.Reminder;
			assignment.ChannelId = context.Channel.Id;
			assignment.Content = content;
			assignment.GuildId = context.Guild.Id;
			assignment.MessageId = context.Message.Id;
			assignment.SetAt = DateTime.Now + setOff;
			assignment.SetOff = DateTime.Now;
			assignment.UserId = context.User.Id;
			_ = await Program.Database.Assignments.AddAsync(assignment);
			_ = await Program.Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Set off at {DateTime.Now.Add(setOff).ToString("MMM dd', 'HHH':'mm':'ss", CultureInfo.InvariantCulture)}: ```\n{content}```");
		}

		[GroupCommand]
		public async Task ListByGroup(CommandContext context) => await List(context);

		[Command("list")]
		[Description("Lists what reminders are set.")]
		public async Task List(CommandContext context)
		{
			Assignment[] tasks = Program.Database.Assignments.Where(assignment => assignment.UserId == context.User.Id).ToArray();
			List<string> reminders = new();
			if (tasks == null) reminders.Add("No reminders are set!");
			else
				foreach (Assignment task in tasks)
					reminders.Add($"Id #{task.Id}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss", CultureInfo.InvariantCulture)}: {task.Content}");
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"All reminders for {context.Member.GetCommonName()}");
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join("\n", reminders.ToArray()), SplitType.Character, embedBuilder), timeoutoverride: TimeSpan.FromMinutes(2));
		}

		[Command("remove"), Description("Removes a reminder.")]
		public async Task Remove(CommandContext context, int taskId)
		{
			Assignment task = Program.Database.Assignments.Where(assignment => assignment.AssignmentType == AssignmentType.Reminder && assignment.UserId == context.User.Id && assignment.Id == taskId).First();
			if (task != null) _ = await Program.SendMessage(context, $"**[Error: Reminder #{taskId} does not exist!]**");
			else
			{
				_ = Program.Database.Assignments.Remove(task);
				_ = await Program.Database.SaveChangesAsync();
				_ = await Program.SendMessage(context, $"Reminder #{taskId} was removed!");
			}
		}

		[Command("remove")]
		public async Task Remove(CommandContext context, string taskId)
		{
			try
			{
				_ = Remove(context, int.Parse(taskId.Remove(0, 1), CultureInfo.InvariantCulture));
			}
			catch (FormatException)
			{
				_ = await Program.SendMessage(context, $"\"{taskId}\" is not an id.");
			}
		}

		public static void StartRoutine()
		{
			Timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
			Timer.Elapsed += async (object sender, ElapsedEventArgs e) =>
			{
				Assignment[] tasks = await Program.Database.Assignments.ToArrayAsync();
				if (tasks == null) return;
				foreach (Assignment task in tasks)
				{
					if (task.SetOff.CompareTo(DateTime.Now) < 0)
					{
						DiscordClient client = Program.Client.ShardClients[0];
						CommandsNextExtension commandsNext = client.GetCommandsNext();
						DiscordGuild guild = await client.GetGuildAsync(task.GuildId);
						DiscordChannel channel = guild.GetChannel(task.ChannelId);
						CommandContext context;
						switch (task.AssignmentType)
						{
							case AssignmentType.Reminder:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["remind"], null);
								_ = await Program.SendMessage(context, $"Set at {task.SetAt.ToString("MMM dd', 'HHH':'mm", CultureInfo.InvariantCulture)}: {task.Content}");
								_ = Program.Database.Assignments.Remove(task);
								break;
							case AssignmentType.TempBan:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["tempban"], null);
								await Moderation.Unban.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								_ = Program.Database.Assignments.Remove(task);
								break;
							case AssignmentType.TempMute:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["tempmute"], null);
								await Moderation.Unmute.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								_ = Program.Database.Assignments.Remove(task);
								break;
							case AssignmentType.TempAntimeme:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Config.Prefix, commandsNext.RegisteredCommands["tempantimeme"], null);
								await Moderation.Promeme.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								_ = Program.Database.Assignments.Remove(task);
								break;
						}
					}
				}
			};
			Timer.Start();
		}
	}
}
