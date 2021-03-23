using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tomoe.Db
{
	public class ModLog
	{
		[Key]
		public int Id { get; internal set; }
		public ulong GuildId { get; internal set; }
		public ModLogType Type { get; internal set; }
		public ModAction Action { get; internal set; }
		public DiscordEvent DiscordEvent { get; internal set; } = DiscordEvent.None;
		public bool TempAction { get; internal set; }
		public ulong IssuerId { get; internal set; }
		public ulong[] IdsAffected { get; internal set; }
		public string Details { get; internal set; }
		public DateTime CreatedAt { get; internal set; } = DateTime.UtcNow;
	}

	public enum ModLogType
	{
		Moderation,
		Discord,
		Other
	}

	/// <summary>
	/// Moderation actions that Tomoe can do.
	/// </summary>
	public enum ModAction
	{
		Banned,
		Kicked,
		Muted,
		Antimemed,
		Voicebanned,
		Striked,
		Unban,
		Unmuted,
		Unantimemed,
		Unvoicebanned,
		ReadLog,
		CommandExecuted,
		DiscordEvent,
		Other
	}

	/// <summary>
	/// DiscordEvents mapped to an enum. Source: https://discord.com/developers/docs/topics/gateway#commands-and-events-gateway-events
	/// </summary>
	public enum DiscordEvent
	{
		None, // Created for default value.
		Ready,
		Resumed,
		Reconnect,
		// Not sure if these will be used due to DSharpPlus refusing to support it.
		SlashCommandCreated,
		SlashCommandUpdated,
		SlashCommandDeleted,
		SlashCommandUsed,
		// End section
		ChannelCreated,
		ChannelUpdated,
		ChannelDeleted,
		MessagePinned,
		GuildCreated,
		GuildUpdated,
		GuildDeleted,
		UserBanned,
		UserUnbanned,
		EmojiUpdated,
		IntegrationsUpdated,
		MemberAdded,
		MemberUpdated,
		MemberRemoved,
		RoleCreated,
		RoleUpdated,
		RoleDeleted,
		InviteCreated,
		InviteDeleted,
		MessageCreated,
		MessageUpdated,
		MessageDeleted,
		MessageBulkDeleted,
		ReactionAdded,
		ReactionRemoved,
		ReactionRemovedAll,
		ReactionRemovedEmoji,
		UserUpdated,
		VoiceChannelJoined,
		VoiceChannelLeft,
		VoiceChannelMoved
	}
}
