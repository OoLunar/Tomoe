namespace Tomoe.Utils
{
	public class Config
	{
		public string DiscordApiToken = string.Empty;
		public string DiscordBotPrefix = ">>";
		public string RepositoryLink = "https://github.com/OoLunar/Tomoe.git";
		public string InviteLink = "https://discord.com/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot";
		public LoggerConfig Logger = new();
		public DatabaseConfig Database = new();
	}
}
