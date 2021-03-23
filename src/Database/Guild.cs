using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tomoe.Commands.Moderation;

namespace Tomoe.Db
{
	public class Guild
	{
		public bool AntiInvite { get; internal set; } = true;
		public bool AutoDehoist { get; internal set; }
		public bool ProgressiveStrikes { get; internal set; } = true;
		public bool StrikeAutomod { get; internal set; }
		public Dictionary<int, ProgressiveStrike> Punishments { get; internal set; } = new();
		public int MaxLines { get; internal set; } = 5;
		public int MaxMentions { get; internal set; } = 5;
		public List<GuildUser> Users { get; internal set; } = new();
		public List<ReactionRole> ReactionRoles { get; internal set; } = new();
		public List<string> AllowedInvites { get; internal set; } = new();
		public List<Tag> Tags { get; internal set; } = new();
		public List<ulong> AdminRoles { get; internal set; } = new();
		public List<ulong> IgnoredChannels { get; internal set; } = new();
		public ulong AntimemeRole { get; internal set; }
		[Key]
		public ulong Id { get; internal set; }
		public ulong MuteRole { get; internal set; }
		public ulong VoicebanRole { get; internal set; }

		public Guild(ulong id) => Id = id;
	}
}
