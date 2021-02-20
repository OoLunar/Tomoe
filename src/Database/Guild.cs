using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
	public class Guild
	{
		[Key]
		public ulong Id { get; internal set; }
		public List<string> AllowedInvites { get; internal set; }
		public bool AntiRaidActivated { get; internal set; } = false;
		public bool AutoRaidMode { get; internal set; } = true;
		public int AntiRaidSetOff { get; internal set; } = 30;
		public bool AntiInvite { get; internal set; } = true;
		public int MaxLines { get; internal set; } = 5;
		public int MaxMentions { get; internal set; } = 5;
		public bool AutoDehoist { get; internal set; } = false;
		public List<ulong> IgnoredChannels { get; internal set; }
		public List<ulong> AdminRoles { get; internal set; }
		public ulong MuteRole { get; internal set; }
		public ulong AntimemeRole { get; internal set; }
		public ulong VoiceBanRole { get; internal set; }

		public List<Tag> Tags { get; internal set; }
		public List<GuildUser> Users { get; internal set; }

		public Guild(ulong id) => this.Id = id;
	}
}
