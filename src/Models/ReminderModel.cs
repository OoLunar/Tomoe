using System;
using System.ComponentModel.DataAnnotations;
using Tomoe.Interfaces;

namespace Tomoe.Models
{
	public class ReminderModel : IExpires<int>
	{
		[Key]
		public int Id { get; init; }
		public ulong UserId { get; init; }
		public Uri MessageLink { get; init; } = null!;
		public string? Content { get; internal set; }
		public DateTime SetAt { get; init; }
		public DateTime ExpiresAt { get; internal set; }
		public bool Reply { get; init; }

		public ulong ChannelId => ulong.Parse(MessageLink.Segments[2]);
		public ulong? GuildId => MessageLink.Segments[1] == "@me" ? ulong.Parse(MessageLink.Segments[1]) : null;
	}
}
