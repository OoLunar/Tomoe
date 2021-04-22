using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class GuildConfig
	{
		public bool AntiInvite { get; internal set; } = true;
		public bool AutoDehoist { get; internal set; }
		public bool ProgressiveStrikes { get; internal set; } = true;
		public bool StrikeAutomod { get; internal set; }
		public int MaxLines { get; internal set; } = 5;
		public int MaxMentions { get; internal set; } = 5;
		public List<string> AllowedInvites { get; internal set; } = new();
		public List<string> Prefixes { get; internal set; } = new();
		public List<ulong> AdminRoles { get; internal set; } = new();
		public List<ulong> IgnoredChannels { get; internal set; } = new();
		public ulong AntimemeRole { get; internal set; }
		[Key]
		public ulong Id { get; internal set; }
		public ulong MuteRole { get; internal set; }
		public ulong VoicebanRole { get; internal set; }

		public GuildConfig(ulong id) => Id = id;
	}
}
