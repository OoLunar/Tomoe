using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tomoe.Interfaces;

namespace Tomoe.Models
{
	public class PollModel : IExpires<Guid>
	{
		[Key]
		public Guid Id { get; init; }
		public ulong GuildId { get; init; }
		public ulong ChannelId { get; init; }
		public ulong MessageId { get; set; }
		public ulong UserId { get; init; }
		public string Question { get; init; } = null!;
		public DateTime ExpiresAt { get; internal set; }
		[Column(TypeName = "jsonb")]
		public Dictionary<string, ulong[]> Votes { get; init; } = new();
	}
}
