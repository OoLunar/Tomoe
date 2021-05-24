namespace Tomoe.Utils.Configs
{
    using System.Text.Json.Serialization;

    public class Update
    {
        [JsonPropertyName("branch")]
        public string Branch { get; set; }

        [JsonPropertyName("notify")]
        public bool Notify { get; set; }

        [JsonPropertyName("auto_update")]
        public bool AutoUpdate { get; set; }

        [JsonPropertyName("guild_id")]
        public ulong GuildId { get; set; }

        [JsonPropertyName("channel_id")]
        public ulong ChannelId { get; set; }

        [JsonPropertyName("user_id")]
        public ulong UserId { get; set; }

        [JsonPropertyName("git_name")]
        public string GitName { get; set; }

        [JsonPropertyName("git_email")]
        public string GitEmail { get; set; }
    }
}
