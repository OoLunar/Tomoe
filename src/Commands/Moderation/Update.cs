namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Tomoe.Utils.Types;

    public class Update : BaseCommandModule
    {
        [Command("update"), RequireOwner, Description("Updates the bot to the latest version. Branch is set in the config.")]
        public async Task ByUser(CommandContext context)
        {
            string latestVersion = await GetLatestVersion();
            if (latestVersion == Constants.Version)
            {
                _ = await Program.SendMessage(context, "Currently on the latest version!");
            }
            else
            {
                if (Program.Config.Update.AutoUpdate)
                {
                    Checklist checklist = new(context, "Downloading latest version...", "Extracting latest version...");
                    await Download(latestVersion);
                    await checklist.Finalize("Rebooting", true);
                }
                else
                {
                    DiscordMessage message = await Program.SendMessage(context, $"Latest version: {latestVersion}. Current version: {Constants.Version}. Would you like to update? Be aware that the files will be downloaded to the current directory and not the project directory!");
                    Queue queue = new(message, context.User, new(async eventArgs =>
                    {
                        if (eventArgs.TimedOut || eventArgs.MessageReactionAddEventArgs.Emoji == Constants.ThumbsDown)
                        {
                            _ = await message.ModifyAsync(Formatter.Strike(Formatter.Strip(message.Content)) + "\nNot updating!");
                        }
                        else
                        {
                            Checklist checklist = new(context, "Downloading latest version...", "Extracting latest version...");
                            await Download(latestVersion);
                            await checklist.Finalize("Rebooting", true);
                        }
                    }));
                }
            }
        }

        public static async Task<string> GetLatestVersion()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/vnd.github.v3+json"));
            string githubLatestVersion = "";
            if (Program.Config.Update.Branch.ToLowerInvariant() == "beta")
            {
                githubLatestVersion = JsonSerializer.Deserialize<JsonElement>(await client.GetStringAsync("https://api.github.com/repos/OoLunar/Mod_Downloader/tags?per_page=1")).GetProperty("0.name").GetString();
            }
            else if (Program.Config.Update.Branch.ToLowerInvariant() == "public")
            {
                githubLatestVersion = JsonSerializer.Deserialize<JsonElement>(await client.GetStringAsync("https://api.github.com/repos/OoLunar/Mod_Downloader/releases/latest")).GetProperty("tag_name").GetString();
            }
            return githubLatestVersion.ToLowerInvariant();
        }

        public static async Task Download(string latestVersion)
        {
            HttpClient client = new();
            string githubDownloadLink = "";
            if (Program.Config.Update.Branch.ToLowerInvariant() == "beta")
            {
                githubDownloadLink = JsonSerializer.Deserialize<JsonElement>(await client.GetStringAsync($"https://api.github.com/repos/OoLunar/Mod_Downloader/tag/{latestVersion}")).GetProperty("0.zipball_url").GetString();
            }
            else if (Program.Config.Update.Branch.ToLowerInvariant() == "public")
            {
                githubDownloadLink = JsonSerializer.Deserialize<JsonElement>(await client.GetStringAsync($"https://api.github.com/repos/OoLunar/Mod_Downloader/releases/{latestVersion}")).GetProperty("zipball_url").GetString();
            }
            Directory.Delete("./*", true);
            File.WriteAllBytes($"Tomoe-{latestVersion}.zip", await client.GetByteArrayAsync(githubDownloadLink));
            ZipArchive zipArchive = ZipFile.Open($"Tomoe-{latestVersion}.zip", ZipArchiveMode.Read);
            zipArchive.ExtractToDirectory("./", true);
            FileStream configFile = File.Open("./res/config.jsonc.prod", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                AllowTrailingCommas = false,
                IgnoreReadOnlyFields = false,
                IgnoreReadOnlyProperties = false,
                IncludeFields = true,
                NumberHandling = JsonNumberHandling.Strict,
                PropertyNamingPolicy = null,
                WriteIndented = true
            };
            await JsonSerializer.SerializeAsync(configFile, Program.Config, jsonSerializerOptions);
            configFile.Close();
        }
    }
}
