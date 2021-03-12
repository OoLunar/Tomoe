using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Tomoe.Utils.Types
{
	/// <summary>
	/// A checklist that is managed through a <see cref="DiscordMessage"/>.
	/// </summary>
	public class Checklist : IDisposable
	{
		/// <summary>
		/// The current line of the <see cref="DiscordMessage"/> we're on.
		/// </summary>
		public int CurrentItem { get; private set; }

		/// <summary>
		/// The lines of the <see cref="DiscordMessage"/> split into a list.
		/// </summary>
		public List<string> Items { get; private set; } = new();

		/// <summary>
		/// The <see cref="DiscordMessage"/> to edit.
		/// </summary>
		private DiscordMessage DiscordMessage;

		/// <summary>
		/// A <see cref="DiscordMessage"/> split line-by-line of what the bot is doing. Each line is prepended with a Discord loading emoji, which is then changed to a check emoji when <see cref="Check()"/> is called, or an x emoji when <see cref="Fail()"/> is called.
		/// </summary>
		/// <param name="context">The <see cref="CommandContext"/> required to send the <see cref="DiscordMessage"/>.</param>
		/// <param name="todoList">The list to check off. Each line will be prepended with a Discord loading emoji.</param>
		public Checklist(CommandContext context, params string[] todoList)
		{
			foreach (string item in todoList)
			{
				Items.Add($"{Constants.Loading} {item}");
			}
			DiscordMessage = Program.SendMessage(context, string.Join('\n', Items)).GetAwaiter().GetResult();
		}

		/// <summary>
		/// A <see cref="DiscordMessage"/> split line-by-line of what the bot is doing. Each line is prepended with a Discord loading emoji, which is then changed to a check emoji when <see cref="Check()"/> is called, or an x emoji when <see cref="Fail()"/> is called.
		/// </summary>
		/// <param name="discordMessage">The <see cref="DiscordMessage"/> to edit. Previous content will be striked out using <see cref="Formatter.Strike(string)"/>, with the <paramref name="todoList"/> on a newline.</param>
		/// <param name="todoList">The list to check off. Each line will be prepended with a Discord loading emoji.</param>
		public Checklist(DiscordMessage discordMessage, params string[] todoList)
		{
			foreach (string item in todoList)
			{
				Items.Add($"{Constants.Loading} {item}");
			}
			DiscordMessage = discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{string.Join('\n', Items)}").GetAwaiter().GetResult();
		}

		/// <summary>
		/// <see cref="Formatter.Strike(string)"/>s the current line and changes the Discord loading emoji to a check emoji.
		/// </summary>
		/// <example>
		/// This will produce a <see cref="DiscordMessage"/> as such:
		/// :check: Get corndog
		/// :loading: Eat corndog
		/// <code>
		/// Checklist checklist = new(context, "Get corndog", "Eat corndog");
		/// await checklist.Check();
		/// </code>
		/// </example>
		public async Task Check()
		{
			Items[CurrentItem] = Formatter.Strike(Items[CurrentItem].Replace(Constants.Loading, Constants.Check));
			CurrentItem++;
			DiscordMessage = await DiscordMessage.ModifyAsync(string.Join('\n', Items));
		}

		/// <summary>
		/// <see cref="Formatter.Strike(string)"/>s the current line and changes the Discord loading emoji to an x emoji.
		/// </summary>
		/// <example>
		/// This will produce a <see cref="DiscordMessage"/> as such:
		/// :check: Get corndog
		/// :x: Eat corndog
		/// <code>
		/// Checklist checklist = new(context, "Get corndog", "Eat corndog");
		/// await checklist.Check();
		/// await checklist.Fail();
		/// </code>
		/// </example>
		public async Task Fail()
		{
			Items[CurrentItem] = Formatter.Strike(Items[CurrentItem].Replace(Constants.Loading, Constants.Failed));
			CurrentItem++;
			DiscordMessage = await DiscordMessage.ModifyAsync(string.Join('\n', Items));
		}

		/// <summary>
		/// <see cref="Formatter.Strike(string)"/>s the current line and changes the Discord loading emoji to a check. Adds the <paramref name="finalMessage"/> to a newline with a prepended check emoji.
		/// </summary>
		/// <example>
		/// This will produce a <see cref="DiscordMessage"/> as such:
		/// :check: Get corndog
		/// :x: Eat corndog
		/// :check: Regret life decisions
		/// <code>
		/// Checklist checklist = new(context, "Get corndog", "Eat corndog");
		/// await checklist.Check();
		/// await checklist.Fail();
		/// await checklist.Finalize("Regret life decisions");
		/// </code>
		/// </example>
		/// <param name="finalMessage">The final line to add to the <see cref="DiscordMessage"/>, prepended with a check emoji.</param>
		public async Task Finalize(string finalMessage)
		{
			await Check();
			DiscordMessage = await DiscordMessage.ModifyAsync($"{DiscordMessage.Content}\n{Constants.Check} {finalMessage}");
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			CurrentItem = 0;
			Items.Clear();
			GC.SuppressFinalize(this);
		}
	}
}
