using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tomoe.Commands.Moderation;

namespace Tomoe.Db
{
	public class Guild
	{
		[Key]
		public ulong Id { get; internal set; }
		public List<string> AllowedInvites { get; internal set; } = new();
		public bool AntiInvite { get; internal set; } = true;
		public int MaxLines { get; internal set; } = 5;
		public int MaxMentions { get; internal set; } = 5;
		public bool AutoDehoist { get; internal set; } = false;
		public List<ulong> IgnoredChannels { get; internal set; } = new();
		public List<ulong> AdminRoles { get; internal set; } = new();
		public ulong MuteRole { get; internal set; }
		public ulong AntimemeRole { get; internal set; }
		public ulong VoicebanRole { get; internal set; }
		public bool StrikeAutomod { get; internal set; } = false;
		public bool ProgressiveStrikes { get; internal set; } = true;

		public Dictionary<int, ProgressiveStrike> Punishments { get; internal set; } = new();
		public List<GuildUser> Users { get; internal set; } = new();
		public List<Tag> Tags { get; internal set; } = new();

		public Guild(ulong id) => Id = id;
	}
}
