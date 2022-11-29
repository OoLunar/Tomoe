using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tomoe.Enums;

namespace Tomoe.Models
{
	public class AutoModel<T> : IAutoModel
	{
		[Key]
		public Guid Id { get; init; }
		public ulong GuildId { get; init; }
		public ulong ChannelId { get; init; }
		public FilterType FilterType { get; init; }
		public string? Filter { get; init; } = null!;
		public List<T> Values { get; init; } = new();
	}
}
