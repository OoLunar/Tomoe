using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Models
{
	public class MemberModel
	{
		[Key]
		public Guid Id { get; init; }
		public ulong UserId { get; init; }
		public ulong GuildId { get; init; }
		public List<ulong> Roles { get; internal set; } = new();
		public bool IsMuted { get; internal set; }
		public bool IsAntimemed { get; internal set; }
		public bool IsVoicebanned { get; internal set; }
		public DateTime JoinedAt { get; init; }
		public bool IsInGuild { get; internal set; }
	}
}