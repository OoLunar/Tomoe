namespace Tomoe
{
	public class Constants
	{
		public const string MissingReason = "**[Notice: No reason was provided.]**";
		public const string SelfPunishment = "**[Warning: You're about to punish yourself. Do you still want to go through with this?]**";
		public const string MissingRole = "**[Error: No role has been set!]**";
		public const string UserNotFound = "**[Error: User not found.]**";
		public const string RawEmbed = "**[Error: Cannot get the raw version of an embed!]**";
		public const string GuildOwner = "**[Denied: Cannot execute actions on the guild owner!]**";
		public const string MissingPermissions = "**[Denied: Missing permissions.]**";
		public const string NotAGuild = "**[Denied: Guild command.]**";
		public const string SelfAction = "**[Denied: Cannot execute on myself.]**";
		public const string Hierarchy = "**[Denied: Prevented by hierarchy.]**";

		public class Tags
		{
			public const string NotFound = "**[Error: No tags found!]**";
			public const string TooLong = "**[Error: Tag title too long.]**";
			public const string AliasesOfAliases = "**[Denied: Creating aliases of aliases aren't allowed. Try getting the real tag name first.]**";
			public const string AuthorStillPresent = "**[Denied: Tag author is still in the guild!]**";
			public const string NotOwnerOf = "**[Denied: You aren't the tag owner!]**";
			public const string NotATag = "**[Denied: Tag isn't a tag.]**";
			public const string NotAnAlias = "**[Denied: Tag isn't an alias.]**";
		}
	}
}
