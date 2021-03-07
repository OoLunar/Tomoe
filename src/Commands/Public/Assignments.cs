using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	[Group("assignment"), Aliases("reminders", "remind", "task"), Description("Creates an assignment to go off at the specified time.")]
	public class Assignments : BaseCommandModule
	{
		public Database Database { private get; set; }
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
			assignment.SetAt = DateTime.UtcNow + setOff;
			assignment.SetOff = DateTime.UtcNow;
			assignment.UserId = context.User.Id;
			_ = Database.Assignments.Add(assignment);
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"{DateTime.UtcNow.Add(setOff).Humanize()}: ```\n{content}```");
		}

		[GroupCommand]
		public async Task ListByGroup(CommandContext context) => await List(context);

		[Command("list")]
		[Description("Lists what assignments are set.")]
		public async Task List(CommandContext context)
		{
			Assignment[] tasks = await Database.Assignments.Where(assignment => assignment.UserId == context.User.Id).ToArrayAsync();
			List<string> reminders = new();
			if (tasks.Length == 0) reminders.Add("No assignments are set!");
			else foreach (Assignment task in tasks)
					reminders.Add($"Id #{task.Id}, Set off at {task.SetOff.ToString("MMM dd', 'HHH':'mm':'ss", CultureInfo.InvariantCulture)}: {task.Content}");
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"All assignments for {context.Member.GetCommonName()}");
			embedBuilder.Author = new()
			{
				Name = context.Guild.Name,
				Url = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl,
				IconUrl = context.Guild.IconUrl ?? context.User.DefaultAvatarUrl
			};
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, interactivity.GeneratePagesInEmbed(string.Join("\n", reminders.ToArray()), SplitType.Character, embedBuilder));
		}

		[Command("remove"), Description("Removes a reminder.")]
		public async Task Remove(CommandContext context, Assignment assignment)
		{
			_ = Database.Assignments.Remove(assignment);
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Reminder #{assignment.Id} was removed!");
		}

		public static void StartRoutine()
		{
			IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Timer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
			Timer.Elapsed += async (object sender, ElapsedEventArgs e) =>
			{
				Assignment[] tasks = await database.Assignments.ToArrayAsync();
				if (tasks.Length == 0) return;
				foreach (Assignment task in tasks)
				{
					if (DateTime.UtcNow > task.SetOff)
					{
						DiscordClient client = Program.Client.ShardClients[0];
						CommandsNextExtension commandsNext = client.GetCommandsNext();
						DiscordGuild guild = await client.GetGuildAsync(task.GuildId);
						DiscordChannel channel = guild.GetChannel(task.ChannelId);
						CommandContext context;
						switch (task.AssignmentType)
						{
							case AssignmentType.Reminder:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Program.Config.DiscordBotPrefix, commandsNext.RegisteredCommands["remind"], null);
								_ = await Program.SendMessage(context, $"{task.SetAt.Humanize()}: {task.Content}");
								_ = database.Assignments.Remove(task);
								break;
							case AssignmentType.TempBan:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Program.Config.DiscordBotPrefix, commandsNext.RegisteredCommands["tempban"], null);
								await Moderation.Unban.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								_ = database.Assignments.Remove(task);
								break;
							case AssignmentType.TempMute:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Program.Config.DiscordBotPrefix, commandsNext.RegisteredCommands["tempmute"], null);
								await Moderation.Unmute.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								_ = database.Assignments.Remove(task);
								break;
							case AssignmentType.TempAntimeme:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Program.Config.DiscordBotPrefix, commandsNext.RegisteredCommands["tempantimeme"], null);
								await Moderation.Promeme.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								_ = database.Assignments.Remove(task);
								break;
							case AssignmentType.TempVoiceBan:
								context = commandsNext.CreateContext(await channel.GetMessageAsync(task.MessageId), Program.Config.DiscordBotPrefix, commandsNext.RegisteredCommands["tempantimeme"], null);
								await Moderation.VoiceBan.ByAssignment(context, await context.Client.GetUserAsync(task.UserId));
								_ = database.Assignments.Remove(task);
								break;
							default: break;
						}
						_ = await database.SaveChangesAsync();
					}
				}
			};
			Timer.Start();
		}
	}
}
