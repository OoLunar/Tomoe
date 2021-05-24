namespace Tomoe.Utils.Configs
{
    using System.Text.Json.Serialization;

    public class Database
    {
        [JsonPropertyName("application_name")]
        public string ApplicationName { get; set; }

        [JsonPropertyName("database_name")]
        public string DatabaseName { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }
    }
}
