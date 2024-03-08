namespace OoLunar.Tomoe.Configuration
{
    public sealed record DiscordConfiguration
    {
        public required string? Token { get; init; }
        public string Prefix { get; init; } = ">>";
        public ulong GuildId { get; init; }
        public string SupportInvite { get; init; } = "https://discord.gg/YCSTr2KGq6";
        public string[] Processors { get; init; } = ["text", "slash", "message", "user"];
    }
}
